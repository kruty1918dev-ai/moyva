using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard028
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_05040();
            Case_05041();
            Case_05042();
            Case_05043();
            Case_05044();
            Case_05045();
            Case_05046();
            Case_05047();
            Case_05048();
            Case_05049();
            Case_05050();
            Case_05051();
            Case_05052();
            Case_05053();
            Case_05054();
            Case_05055();
            Case_05056();
            Case_05057();
            Case_05058();
            Case_05059();
            Case_05060();
            Case_05061();
            Case_05062();
            Case_05063();
            Case_05064();
            Case_05065();
            Case_05066();
            Case_05067();
            Case_05068();
            Case_05069();
            Case_05070();
            Case_05071();
            Case_05072();
            Case_05073();
            Case_05074();
            Case_05075();
            Case_05076();
            Case_05077();
            Case_05078();
            Case_05079();
            Case_05080();
            Case_05081();
            Case_05082();
            Case_05083();
            Case_05084();
            Case_05085();
            Case_05086();
            Case_05087();
            Case_05088();
            Case_05089();
            Case_05090();
            Case_05091();
            Case_05092();
            Case_05093();
            Case_05094();
            Case_05095();
            Case_05096();
            Case_05097();
            Case_05098();
            Case_05099();
            Case_05100();
            Case_05101();
            Case_05102();
            Case_05103();
            Case_05104();
            Case_05105();
            Case_05106();
            Case_05107();
            Case_05108();
            Case_05109();
            Case_05110();
            Case_05111();
            Case_05112();
            Case_05113();
            Case_05114();
            Case_05115();
            Case_05116();
            Case_05117();
            Case_05118();
            Case_05119();
            Case_05120();
            Case_05121();
            Case_05122();
            Case_05123();
            Case_05124();
            Case_05125();
            Case_05126();
            Case_05127();
            Case_05128();
            Case_05129();
            Case_05130();
            Case_05131();
            Case_05132();
            Case_05133();
            Case_05134();
            Case_05135();
            Case_05136();
            Case_05137();
            Case_05138();
            Case_05139();
            Case_05140();
            Case_05141();
            Case_05142();
            Case_05143();
            Case_05144();
            Case_05145();
            Case_05146();
            Case_05147();
            Case_05148();
            Case_05149();
            Case_05150();
            Case_05151();
            Case_05152();
            Case_05153();
            Case_05154();
            Case_05155();
            Case_05156();
            Case_05157();
            Case_05158();
            Case_05159();
            Case_05160();
            Case_05161();
            Case_05162();
            Case_05163();
            Case_05164();
            Case_05165();
            Case_05166();
            Case_05167();
            Case_05168();
            Case_05169();
            Case_05170();
            Case_05171();
            Case_05172();
            Case_05173();
            Case_05174();
            Case_05175();
            Case_05176();
            Case_05177();
            Case_05178();
            Case_05179();
            Case_05180();
            Case_05181();
            Case_05182();
            Case_05183();
            Case_05184();
            Case_05185();
            Case_05186();
            Case_05187();
            Case_05188();
            Case_05189();
            Case_05190();
            Case_05191();
            Case_05192();
            Case_05193();
            Case_05194();
            Case_05195();
            Case_05196();
            Case_05197();
            Case_05198();
            Case_05199();
            Case_05200();
            Case_05201();
            Case_05202();
            Case_05203();
            Case_05204();
            Case_05205();
            Case_05206();
            Case_05207();
            Case_05208();
            Case_05209();
            Case_05210();
            Case_05211();
            Case_05212();
            Case_05213();
            Case_05214();
            Case_05215();
            Case_05216();
            Case_05217();
            Case_05218();
            Case_05219();
        }

        private static void Case_05040()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5040,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,2,95,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,6,29,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,11,11,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,3,16,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,13,89,28,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 20,
                stableHash: "07b827015a0d52b7538b39e7f3a856ef7aa1c318b2fa8dda58d3b7403f62c50f");
        }

        private static void Case_05041()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5041,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-13,71,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,2,62,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,13,13,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-20,21,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,20,88,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,19,31,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-20,66,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,16,72,1,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "12a4124c0435a531c9b093f14d2f72903e1987b28b02ce2ac6802942c61db4a5");
        }

        private static void Case_05042()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5042,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,9,52,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,19,59,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-20,95,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-20,50,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-18,40,23,4), new GeneratedEnemyUnit(5,4,89,37,3), new GeneratedEnemyUnit(-18,20,35,27,1), new GeneratedEnemyUnit(17,-20,71,24,4), new GeneratedEnemyUnit(-17,11,77,20,1), new GeneratedEnemyUnit(20,-19,87,2,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "1e77d4013ed379d1ef5b1d75613cfd060052778ba94e1b700acb29e231f57578");
        }

        private static void Case_05043()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5043,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-14,46,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-10,22,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,10,7,37,4), new GeneratedEnemyUnit(15,-7,61,22,3), new GeneratedEnemyUnit(-14,-14,49,12,1), new GeneratedEnemyUnit(19,-1,56,21,1), new GeneratedEnemyUnit(-14,13,81,41,1), new GeneratedEnemyUnit(6,-1,44,42,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "bb36c20f7e9f750129416ac21b90ab4304d34844110d3643f005f57d8f4cc744");
        }

        private static void Case_05044()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5044,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,14,5,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,16,90,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-6,24,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,8,56,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,10,51,40,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "274682220a0240c15bf2594009be02b4a5fc7242d478212e9c8a10e573c02936");
        }

        private static void Case_05045()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5045,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-10,21,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,12,25,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,16,19,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-13,42,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,18,37,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-3,40,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-5,94,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-15,93,21,2), new GeneratedEnemyUnit(20,0,22,46,1), new GeneratedEnemyUnit(7,17,62,14,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "89660f6721e39b1e92a50f45f5454361da63e5c278561d6dde9beca43f8aec51");
        }

        private static void Case_05046()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5046,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-12,85,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,2,72,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-13,37,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-18,100,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,14,33,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,20,29,4,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "3cd274a611d8a02f3759832824b10505150e0b1cf7fe1e6c5e19a96ff82e5e78");
        }

        private static void Case_05047()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5047,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-19,65,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,0,93,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-3,51,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,18,27,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,8,49,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,19,34,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,7,52,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-5,75,12,1), new GeneratedEnemyUnit(-7,-10,67,35,3), new GeneratedEnemyUnit(4,-12,11,39,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "7e14abe092bd6e2778fd0ad4cd22ff67d4057b850d501910fa861958462cb59c");
        }

        private static void Case_05048()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5048,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,12,48,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,14,34,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,14,36,23,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "961b7970adda4afa4ec62b2f6c1b76df2ac1e25f78f6e3236a4dd2705b05a68a");
        }

        private static void Case_05049()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5049,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,2,89,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,1,42,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-14,55,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,20,32,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,9,94,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,0,17,17,2), new GeneratedEnemyUnit(-17,18,99,38,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "df722e4cbd2fa19219d13f44523058668d8e18e262612a424acb3cbd138bac50");
        }

        private static void Case_05050()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5050,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,5,78,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-7,51,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,3,97,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-9,82,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-17,81,8,1), new GeneratedEnemyUnit(-6,11,71,15,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 80,
                stableHash: "6dad1d1494412c00ffd16755af993689c79aa164420ad80f47e5f9f297c54f86");
        }

        private static void Case_05051()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5051,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,19,10,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-14,85,33,1), new GeneratedEnemyUnit(-3,8,8,42,4), new GeneratedEnemyUnit(13,13,88,39,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "d0832777e783b76efb44d4b5ef0c663f76911f86081e18dd580df0a1906d8b7b");
        }

        private static void Case_05052()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5052,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-13,39,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,17,83,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-11,99,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,7,18,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,3,61,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-6,72,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,10,94,1,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "a45ab6c9d511aa952b8770de036e2158d2182aab9acf16980bf425581af491f4");
        }

        private static void Case_05053()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5053,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-20,67,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-20,91,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,11,50,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,15,92,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,8,82,7,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,20,78,49,1), new GeneratedEnemyUnit(-7,-20,53,16,4), new GeneratedEnemyUnit(11,18,77,12,3), new GeneratedEnemyUnit(-14,-10,83,16,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "3062b4873a5ec1dbfb2c4ede9ce8bd6c4f2a013da32f6bbefd263e12c18520a7");
        }

        private static void Case_05054()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5054,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,14,71,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-1,11,28,3), new GeneratedEnemyUnit(-17,-12,62,10,1), new GeneratedEnemyUnit(-11,7,91,5,4), new GeneratedEnemyUnit(0,-20,82,31,4), new GeneratedEnemyUnit(-4,-12,100,3,3), new GeneratedEnemyUnit(7,-9,74,6,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "89920511f3671c4d72c361f9f3d5f75b68f2af4f441b958d6f8bc633e7b0ec95");
        }

        private static void Case_05055()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5055,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-6,60,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-12,53,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,1,82,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-15,56,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-6,54,14,4), new GeneratedEnemyUnit(-17,4,25,47,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "4ec813c8b76c64b07a2f42ed1f50c93739189587d8362c3536d8a663fc9c99c8");
        }

        private static void Case_05056()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5056,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,8,8,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,0,23,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-9,92,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,1,68,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,11,77,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,2,72,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-18,57,38,1), new GeneratedEnemyUnit(7,-10,54,20,4), new GeneratedEnemyUnit(-10,6,7,37,3), new GeneratedEnemyUnit(7,8,24,41,2), new GeneratedEnemyUnit(-6,3,97,31,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "df18761de01193f011d9a4900cf992182e40368fd2274d853d0d3c77f1fa2c04");
        }

        private static void Case_05057()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5057,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,9,10,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,19,53,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-16,72,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,3,60,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-12,26,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-2,36,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,12,76,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,20,42,11,2), new GeneratedEnemyUnit(6,-9,17,47,3), new GeneratedEnemyUnit(-11,11,50,27,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "670584aa51d6ffb1f3d95047f386f09eaead50f7fe85bf7535d30214d9d1848c");
        }

        private static void Case_05058()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5058,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-18,97,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-6,91,33,1), new GeneratedEnemyUnit(12,14,91,11,2), new GeneratedEnemyUnit(-10,9,65,28,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "fca34eca0ce519abc9f5af6d5bd96d0ddf90b19e88b37291da6b34c75d109c31");
        }

        private static void Case_05059()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5059,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,10,49,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-8,66,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,18,8,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-14,71,34,3), new GeneratedEnemyUnit(18,18,61,44,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "9bb7f4eff755e83cb41e5df2b0f55927c4ad9dd658b60945b60ff11cb2202da8");
        }

        private static void Case_05060()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5060,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,20,7,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-19,84,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-18,66,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-9,16,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,17,44,43,4), new GeneratedEnemyUnit(12,-12,78,41,2), new GeneratedEnemyUnit(-12,17,24,48,2), new GeneratedEnemyUnit(0,-6,96,16,2), new GeneratedEnemyUnit(17,6,74,39,1), new GeneratedEnemyUnit(9,5,69,34,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "ea8b229909dfb6ba953af98bf4f4b7017c7a96a7f3b5aa30a599c8fea9932f00");
        }

        private static void Case_05061()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5061,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-4,50,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-17,93,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,0,59,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,6,52,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,3,17,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-8,39,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,3,62,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-15,12,23,1), new GeneratedEnemyUnit(16,6,20,48,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "f267c8003b99680689521f67bed42ca895fdeee5f2cecfd63088280ce366f314");
        }

        private static void Case_05062()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5062,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,15,37,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,6,75,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-4,12,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-11,49,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,14,90,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-5,14,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,8,74,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-7,84,33,3), new GeneratedEnemyUnit(4,-14,84,6,3), new GeneratedEnemyUnit(11,3,93,2,3), new GeneratedEnemyUnit(3,-15,36,12,2), new GeneratedEnemyUnit(-2,12,97,49,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "2cb9f62ae2a2bd41ee67643e0ee168b7f4bc2aad4f6824051a404caa0940901a");
        }

        private static void Case_05063()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5063,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,13,26,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,19,19,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,6,34,16,2), new GeneratedEnemyUnit(4,12,16,32,4), new GeneratedEnemyUnit(6,2,76,9,2), new GeneratedEnemyUnit(-1,3,83,14,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "f1571eed3bccede04989aa9fea1942c37cc61135de3174c5ff4f76ddb03a7cbb");
        }

        private static void Case_05064()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5064,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,19,37,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,5,8,21,2), new GeneratedEnemyUnit(10,9,100,40,4), new GeneratedEnemyUnit(-19,-5,55,20,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "7b9e96d761e6c4becf8ca637da8c55f895acccefc91481692d24b271d47d6ac3");
        }

        private static void Case_05065()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5065,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-8,59,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,6,54,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-7,25,30,3), new GeneratedEnemyUnit(-9,-17,97,26,2), new GeneratedEnemyUnit(-10,-9,14,17,2), new GeneratedEnemyUnit(-10,0,33,7,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "9561b29bfe80d2d5498fa2d5b18e22ba1a04122623dd95cbb2ad26cb54d3f4ff");
        }

        private static void Case_05066()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5066,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-12,9,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,17,98,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-4,95,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,6,62,21,2), new GeneratedEnemyUnit(6,4,77,2,1), new GeneratedEnemyUnit(7,-3,53,25,3), new GeneratedEnemyUnit(-7,14,17,32,3), new GeneratedEnemyUnit(2,20,42,34,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "b3e250b6a6928b863a3eec9f38ebe28ea08391c76e906c2ee0a997737d23b76d");
        }

        private static void Case_05067()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5067,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,8,44,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-6,32,39,2), new GeneratedEnemyUnit(16,-16,50,41,2), new GeneratedEnemyUnit(-1,19,100,35,4), new GeneratedEnemyUnit(15,18,41,20,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "8826f9502f3bb718eff53cafc805cc14382d8c06545dc9b2f14090267a137a7e");
        }

        private static void Case_05068()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5068,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-11,25,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-19,26,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,17,91,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,2,22,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-12,97,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,9,94,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-4,44,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-14,15,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-20,36,47,2), new GeneratedEnemyUnit(-9,-6,29,20,3), new GeneratedEnemyUnit(-9,18,23,49,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "0a08143a79409a9dbb4637229a9dad1ef851633291729d4338454657c1413b03");
        }

        private static void Case_05069()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5069,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,0,62,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,18,65,5,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-17,81,15,4), new GeneratedEnemyUnit(19,18,86,3,1), new GeneratedEnemyUnit(1,-7,71,48,3), new GeneratedEnemyUnit(-9,8,44,30,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "d4f3988058cad962d86aa14cc6d9d0dffcc4eb90577202b7244a2d17fb3a4a8d");
        }

        private static void Case_05070()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5070,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-15,40,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,10,16,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-5,34,49,3), new GeneratedEnemyUnit(-11,9,13,4,2), new GeneratedEnemyUnit(18,17,96,26,3), new GeneratedEnemyUnit(-4,15,82,8,1), new GeneratedEnemyUnit(-4,16,10,35,3), new GeneratedEnemyUnit(15,13,31,24,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "8871bf60447ef44bc9112fedc15ddf328cebd7c13ea31e6a3b437e4505e1c6ef");
        }

        private static void Case_05071()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5071,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,3,60,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,6,8,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,14,86,5,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "62ab30c1f16f45fe96fda832a65011ba16e40d2e19acb95e76d17e390f4bdd99");
        }

        private static void Case_05072()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5072,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-15,93,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,11,78,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,7,26,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,9,21,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-19,34,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,2,28,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,20,59,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,10,23,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,3,84,19,3), new GeneratedEnemyUnit(-1,-4,78,7,2), new GeneratedEnemyUnit(-15,16,46,28,2), new GeneratedEnemyUnit(2,3,10,5,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "e1311d4edb154ab42f7d4a774ea070d4ebd37b8ea055c4e705a77972f3978c78");
        }

        private static void Case_05073()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5073,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-19,18,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,9,34,19,2), new GeneratedEnemyUnit(-10,-9,56,35,1), new GeneratedEnemyUnit(7,-8,17,3,3), new GeneratedEnemyUnit(-17,9,52,23,2), new GeneratedEnemyUnit(-3,-11,62,31,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "d88cf1d408063d66cdb15d210fdfbd4224f3cef73ab66c23779aedf25a764f09");
        }

        private static void Case_05074()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5074,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-7,93,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-14,8,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-6,83,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-5,76,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-3,24,4,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "c2aba279d5ae992f1ed3f7cebbe7a9ddb8d84ca3ad39f0fc3d24932f33039cb5");
        }

        private static void Case_05075()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5075,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,10,24,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-6,64,12,2), new GeneratedEnemyUnit(10,16,86,27,2), new GeneratedEnemyUnit(-9,-8,18,11,1), new GeneratedEnemyUnit(9,4,64,9,1), new GeneratedEnemyUnit(-13,15,100,35,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "8119af5616308a39149366a0bb8221988f791ffa22345d2699339ebe0ded0001");
        }

        private static void Case_05076()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5076,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-5,23,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,10,86,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,0,97,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,2,16,4,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "9f149ba042cbfa975be85b025ff3138d4bb9cf3edd94888cf14e574c3efdb6a5");
        }

        private static void Case_05077()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5077,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,9,19,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,15,97,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-16,57,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,6,6,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,12,84,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,15,39,25,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "1174f5363fc05ee5eccc05e562d19ebbdfbb5216a8b325bef2bd41ef63631b0e");
        }

        private static void Case_05078()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5078,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-7,93,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-5,48,44,4), new GeneratedEnemyUnit(-8,-2,19,49,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "1b60895d7db50888c526a447d54f4a26904eabb9b060d49b65b676a40911c83b");
        }

        private static void Case_05079()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5079,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-13,62,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,2,85,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-16,7,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,0,55,45,4), new GeneratedEnemyUnit(-9,16,80,11,3), new GeneratedEnemyUnit(-19,6,37,16,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "a5eea373bb12c6ecff1393819dbbadffdce591589dd92d3e50423380934123f9");
        }

        private static void Case_05080()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5080,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,13,45,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,0,5,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-19,38,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-20,81,32,4), new GeneratedEnemyUnit(19,8,11,27,1), new GeneratedEnemyUnit(-19,0,51,48,4), new GeneratedEnemyUnit(-13,4,55,4,2), new GeneratedEnemyUnit(-13,3,72,34,1), new GeneratedEnemyUnit(13,-19,25,13,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "b6fe894edc7f696748498cc1f4b83068a85c197e09abcd9e6d8e29b36249e496");
        }

        private static void Case_05081()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5081,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,1,36,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,4,31,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-11,96,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-19,57,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-7,14,36,2), new GeneratedEnemyUnit(-11,0,42,14,1), new GeneratedEnemyUnit(-10,-18,77,39,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "2ddde41c395b64a4bbbd5a7b5fb0d42262d4450b85cdbd4adcbdf9233e2a5c7e");
        }

        private static void Case_05082()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5082,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,7,87,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,0,30,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-4,44,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,9,96,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,11,9,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,13,100,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-9,10,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,6,88,25,2), new GeneratedEnemyUnit(12,6,37,40,3), new GeneratedEnemyUnit(-14,0,83,20,2), new GeneratedEnemyUnit(-17,-5,82,6,1), new GeneratedEnemyUnit(-8,-9,50,26,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "73fed88f21185578e4e4c4841fd8251cdf02881371e8c2b2e3b6f05013416482");
        }

        private static void Case_05083()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5083,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-14,97,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-2,66,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,5,61,28,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "4fb2c10c332367341c7175f6c6190226f381b40a52b0b2d9d782fb0a501a1c18");
        }

        private static void Case_05084()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5084,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-12,64,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,8,80,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-7,19,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-1,23,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-7,32,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,7,43,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,8,27,29,4), new GeneratedEnemyUnit(-19,3,9,35,4), new GeneratedEnemyUnit(-1,-17,88,10,3), new GeneratedEnemyUnit(2,-12,60,42,1), new GeneratedEnemyUnit(16,-13,70,12,1), new GeneratedEnemyUnit(19,3,90,47,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "3733652e6d13edefc6c58bceb1b85f43e8b3540d2b35cc931b65f6cc7cba4949");
        }

        private static void Case_05085()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5085,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,2,25,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-17,15,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-7,37,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,9,64,49,4), new GeneratedEnemyUnit(9,-5,13,40,1), new GeneratedEnemyUnit(-20,15,46,22,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "5791c355cd50abbba14cc5fb0cc1389a21e1dd8301256d4f99559e90a3721dcf");
        }

        private static void Case_05086()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5086,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-7,42,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-15,69,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-6,74,26,4), new GeneratedEnemyUnit(17,11,14,15,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 80,
                stableHash: "f768964e4d29bea915f874dc02b16554cc7787f277be2e6e78a53566014a4205");
        }

        private static void Case_05087()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5087,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-20,46,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,19,39,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-5,56,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-17,95,16,1), new GeneratedEnemyUnit(14,8,41,21,2), new GeneratedEnemyUnit(8,-10,78,43,3), new GeneratedEnemyUnit(-14,-6,70,49,3), new GeneratedEnemyUnit(-5,-17,64,27,4), new GeneratedEnemyUnit(-4,17,9,44,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "73557c96f7b0e9303d8dc1f60fe090cce32be9ce83849285073268d36bb810e6");
        }

        private static void Case_05088()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5088,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,9,45,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,4,65,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,18,25,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,14,93,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-2,96,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,3,28,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,19,47,14,2), new GeneratedEnemyUnit(-14,-2,100,9,4), new GeneratedEnemyUnit(4,-15,86,48,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "fb9263a14e850622e3073a4f3790d50d9802f3899e17f0f5eacc704a6d58fdac");
        }

        private static void Case_05089()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5089,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-10,38,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,8,30,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,19,63,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-7,29,34,1), new GeneratedEnemyUnit(11,7,41,13,4), new GeneratedEnemyUnit(8,5,20,50,2), new GeneratedEnemyUnit(3,19,93,32,4), new GeneratedEnemyUnit(15,9,57,23,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "b3d2586b3aaf6d00cd84bf8049910b69ad8b3f4420a8b3bd0b7eb8ea43020e1d");
        }

        private static void Case_05090()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5090,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-5,40,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-19,40,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-13,29,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-14,44,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-5,96,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,2,94,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,16,20,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,16,92,20,4), new GeneratedEnemyUnit(-19,9,75,15,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "c4d18a9b28abc66217846205a9283ec92c3d40e23efb6ac490a4ef33a4a4e56f");
        }

        private static void Case_05091()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5091,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-6,51,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,9,58,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,5,42,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,14,27,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,15,68,13,3), new GeneratedEnemyUnit(-17,-18,73,21,1), new GeneratedEnemyUnit(16,10,86,28,4), new GeneratedEnemyUnit(19,16,72,12,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "4ff0e8b419582ca9ef926aa9e402eec3796a37c1a3df37ae5c06af11e92cfd5e");
        }

        private static void Case_05092()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5092,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-16,82,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-3,98,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,19,35,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,17,38,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-16,77,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-7,28,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-19,96,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,10,83,35,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "ec86d277aad8133d69d7d12d61b86e580a40ae50cc784ee6b657b071f1d9defc");
        }

        private static void Case_05093()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5093,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,14,88,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-11,51,40,4), new GeneratedEnemyUnit(-4,2,34,32,3), new GeneratedEnemyUnit(-1,15,41,2,2), new GeneratedEnemyUnit(-20,8,30,14,2), new GeneratedEnemyUnit(7,14,41,16,2), new GeneratedEnemyUnit(-12,-19,77,44,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "936a8323f51a491a2fb20763d4173861f2ecad9bab8061d3129d35464bc32360");
        }

        private static void Case_05094()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5094,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,18,60,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,13,19,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,7,55,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-20,100,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,18,20,2,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,15,39,30,1), new GeneratedEnemyUnit(9,-12,73,38,4), new GeneratedEnemyUnit(4,19,26,10,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "4957c6f2f9b71521e7f00935cc4c91af9f87f2907c524ae7b4ca044136d90be7");
        }

        private static void Case_05095()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5095,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,13,61,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,5,26,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,18,77,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,12,89,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-9,14,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-1,42,4,4), new GeneratedEnemyUnit(0,6,55,13,4), new GeneratedEnemyUnit(-18,16,75,46,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "ac4239217ca2c19f55334aa5a46341387fb8570ced19a860f3a71f343bc6de03");
        }

        private static void Case_05096()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5096,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-7,19,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,12,59,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,1,99,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,10,47,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,4,32,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,17,98,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,3,86,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-1,31,38,1), new GeneratedEnemyUnit(16,18,34,22,1), new GeneratedEnemyUnit(-10,14,64,27,1), new GeneratedEnemyUnit(5,20,89,36,2), new GeneratedEnemyUnit(13,1,68,50,1), new GeneratedEnemyUnit(-9,-9,61,13,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "7f0a52412df55b53aa8dc97eaca390c2e28f787917b2a0746e2a292f94d7cd71");
        }

        private static void Case_05097()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5097,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-19,72,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,10,46,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,2,82,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-9,37,11,2), new GeneratedEnemyUnit(-20,-9,37,2,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "9d63624ddc54df4ea5ef4147c5095d34e7011f6c252da6b4845d701231bc5569");
        }

        private static void Case_05098()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5098,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-13,100,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,20,20,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-8,23,24,4), new GeneratedEnemyUnit(5,-17,73,27,4), new GeneratedEnemyUnit(11,-18,19,8,2), new GeneratedEnemyUnit(-19,-6,74,44,4), new GeneratedEnemyUnit(16,14,15,12,3), new GeneratedEnemyUnit(-6,10,66,4,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "83cfcc98307a47dec1ef55ad6e470d6c6d10e75da204d5f0e29e0bfad81ab7a4");
        }

        private static void Case_05099()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5099,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,9,77,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,16,54,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,12,81,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-12,33,17,3), new GeneratedEnemyUnit(-2,-3,43,40,1), new GeneratedEnemyUnit(1,17,55,45,2), new GeneratedEnemyUnit(12,-7,67,17,2), new GeneratedEnemyUnit(13,2,67,35,1), new GeneratedEnemyUnit(20,-20,69,38,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "651fbc7057240d5b8c04e87971fc1b15367648aac66c7e8c8af3a2dc23b1aa52");
        }

        private static void Case_05100()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5100,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,10,43,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,5,71,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-9,42,9,3), new GeneratedEnemyUnit(13,3,38,45,1), new GeneratedEnemyUnit(9,-5,43,16,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "d70eb60830d68b6d7d4be1a62595e5f79e61375c11fdb565f2676b4f6dbd3b19");
        }

        private static void Case_05101()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5101,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-8,82,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,13,10,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-6,66,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-15,91,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,1,79,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-2,65,48,4), new GeneratedEnemyUnit(20,-4,58,9,1), new GeneratedEnemyUnit(-16,-17,54,20,2), new GeneratedEnemyUnit(-20,-4,58,27,4), new GeneratedEnemyUnit(-17,17,11,49,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 55,
                stableHash: "114f30d74c24fd7fd36e60429f64b5e6199349e5d402d6c4ebbe317e83d47334");
        }

        private static void Case_05102()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5102,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,5,99,4,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "0a4f1c319cae2bedaf164b0c1131fd8fe9dca329ec655d7d1879e186d90b2fef");
        }

        private static void Case_05103()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5103,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-5,8,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-20,88,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-7,87,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-19,32,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,5,72,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,4,35,5,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "23f06174b9728840bc75e8b1f06d0926aef0a5d5707cc0793be10c3ec660b7d5");
        }

        private static void Case_05104()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5104,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,1,90,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,18,54,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,12,81,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,2,30,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,15,71,43,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "ca842a9be8e563af50009f3dcc1db9bce3721c1da45d36420d50e01510928612");
        }

        private static void Case_05105()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5105,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,18,58,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,14,48,7,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "a8f2217b97c4de4921d2c5b2f80ee573ec3fce988927de7e766b02774157ecfd");
        }

        private static void Case_05106()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5106,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-16,65,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-4,62,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-18,42,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-4,6,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-2,45,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-2,60,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-7,47,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-12,90,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-13,32,28,3), new GeneratedEnemyUnit(2,0,32,50,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 80,
                stableHash: "8ccf971716714bbfb5a37a1a566629b6d6660b8f48c5bd91d7518d5775f075f4");
        }

        private static void Case_05107()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5107,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,9,9,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-2,25,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-13,48,1,1), new GeneratedEnemyUnit(4,-8,52,11,3), new GeneratedEnemyUnit(4,-7,24,44,3), new GeneratedEnemyUnit(-13,-5,78,29,1), new GeneratedEnemyUnit(-11,-10,99,37,3), new GeneratedEnemyUnit(-11,15,89,46,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "9ee31b88e2a57c2226579cb90e1b1655e5e1a7aac736b8b2333feddaaf17547c");
        }

        private static void Case_05108()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5108,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,0,35,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-5,13,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-8,19,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,13,8,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,2,94,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-16,59,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-7,41,29,2), new GeneratedEnemyUnit(4,8,34,9,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "ae9b90333aaad52e026d46991907867aa0fcd36662ea0275e1175da0f8942216");
        }

        private static void Case_05109()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5109,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-4,94,6,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-6,63,22,1), new GeneratedEnemyUnit(-10,17,46,28,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "64309c3b8be93f4a8a6f905798427d4e1b1d0af445d939f41856f36ae49ef948");
        }

        private static void Case_05110()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5110,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-14,47,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,14,33,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,4,67,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-9,51,1,4), new GeneratedEnemyUnit(-1,-6,82,35,3), new GeneratedEnemyUnit(8,0,29,44,3), new GeneratedEnemyUnit(5,-2,66,24,3), new GeneratedEnemyUnit(3,10,90,27,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "561c8e76779cb229c62b825f28dc601a413f3e8cb8611364a1c08dbbc44653ff");
        }

        private static void Case_05111()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5111,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-11,95,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-19,77,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,19,38,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,4,89,37,4), new GeneratedEnemyUnit(7,13,50,13,1), new GeneratedEnemyUnit(-20,10,16,27,1), new GeneratedEnemyUnit(5,-1,48,47,3), new GeneratedEnemyUnit(-7,-9,55,48,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "4080ae858be5bdad02d7a06faf2fac55e7148ece0133cc1d5490c17a8a54fea7");
        }

        private static void Case_05112()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5112,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,6,22,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-6,52,49,3), new GeneratedEnemyUnit(-7,-2,35,1,4), new GeneratedEnemyUnit(10,16,81,12,1), new GeneratedEnemyUnit(17,-10,48,46,4), new GeneratedEnemyUnit(-11,0,14,9,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "9632fda45f91c2e75b09bf638704cde0ff62517d52950bccfa40b67fcb925ce7");
        }

        private static void Case_05113()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5113,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,4,51,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,1,44,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,1,13,23,3), new GeneratedEnemyUnit(14,-16,75,39,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "61bf59eb05ea9cd0c41e38041317a5e32e18d0f3d78e2ffac9b6ed79a06ee933");
        }

        private static void Case_05114()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5114,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-18,44,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,2,34,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-15,11,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-11,60,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,5,52,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,4,82,4,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,8,91,10,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "81ec73901b3f6aab422ba5748bf9c54cd2beab9eb4706ebcdb6e5abefc8ff231");
        }

        private static void Case_05115()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5115,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-19,12,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,20,7,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-20,99,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-20,12,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,7,73,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-7,10,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,18,93,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,15,80,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-13,81,32,2), new GeneratedEnemyUnit(-9,-8,57,4,3), new GeneratedEnemyUnit(-1,-9,61,40,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "857d6db3dee92b5495de4da46759af0bc0d63300f9b2cd5076c216fec5a509e5");
        }

        private static void Case_05116()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5116,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-3,36,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,16,57,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,11,86,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,1,92,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,3,43,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,2,70,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,9,91,33,2), new GeneratedEnemyUnit(-4,-13,6,23,3), new GeneratedEnemyUnit(-1,-2,94,49,3), new GeneratedEnemyUnit(-7,-17,75,19,2), new GeneratedEnemyUnit(-15,7,98,46,3), new GeneratedEnemyUnit(-12,-11,88,38,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "7950d752d39130d789e0b8e4c84020c78bc69cea7adae471b33a349ec780ffb7");
        }

        private static void Case_05117()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5117,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-5,92,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,11,32,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,16,41,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,14,26,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,3,50,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-13,25,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-4,46,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,19,50,40,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "c8a885335d75ac1dc84827692f8cb0adabf04d3bdbeccdc127d5669136f198d2");
        }

        private static void Case_05118()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5118,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-10,96,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,8,45,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,11,27,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,20,53,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,11,68,31,2), new GeneratedEnemyUnit(3,-6,36,12,4), new GeneratedEnemyUnit(-12,8,39,12,3), new GeneratedEnemyUnit(17,-9,10,14,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "f0c01a8c8e534e1cf94798bdff3519efcd7d1a54e740d17287c5d12f02736ef5");
        }

        private static void Case_05119()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5119,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,11,72,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,1,100,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-8,45,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-19,30,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-19,33,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-4,53,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,6,100,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,15,31,48,2), new GeneratedEnemyUnit(16,5,79,1,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "597638a71747270c1a2b651d54fcbaec6d2e21c86abf92feb26fdea0482810f1");
        }

        private static void Case_05120()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5120,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,7,56,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,4,72,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,13,76,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-10,96,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,11,30,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,17,6,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,6,68,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-13,6,17,3), new GeneratedEnemyUnit(-15,-17,24,37,2), new GeneratedEnemyUnit(-16,0,84,9,1), new GeneratedEnemyUnit(6,8,32,49,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "53fcb102a90f400390fb5cdfe46ca9722dd12453467d8583a74c641f0313061f");
        }

        private static void Case_05121()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5121,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-11,30,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,9,57,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,0,84,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,15,53,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-13,84,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-5,84,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,2,33,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-1,30,47,4), new GeneratedEnemyUnit(8,20,69,38,3), new GeneratedEnemyUnit(0,2,60,15,3), new GeneratedEnemyUnit(20,0,96,23,2), new GeneratedEnemyUnit(17,-19,83,1,3), new GeneratedEnemyUnit(-14,-1,26,2,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "8056a20c4923ff617ac8177bccd5b73e409114d6302f891d3a3aee7839db4fc9");
        }

        private static void Case_05122()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5122,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,18,46,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-11,44,25,3), new GeneratedEnemyUnit(-19,15,18,33,1), new GeneratedEnemyUnit(8,-16,87,25,3), new GeneratedEnemyUnit(7,-4,92,34,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "69451c6a2d131d9284457f3b6970768bf1ed5f4e684adaab42d45ef4a20eaf20");
        }

        private static void Case_05123()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5123,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-1,50,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-19,56,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,11,94,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,10,71,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-11,5,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-11,76,45,4), new GeneratedEnemyUnit(11,17,76,22,2), new GeneratedEnemyUnit(-18,4,48,17,1), new GeneratedEnemyUnit(-11,14,52,32,2), new GeneratedEnemyUnit(4,12,55,10,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "34a29efdbea240293755372fe046c7a002de74f037db795d32af90a8ac33823a");
        }

        private static void Case_05124()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5124,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-16,100,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,8,19,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-17,57,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-4,41,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,6,99,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,11,20,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,18,69,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-3,85,4,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "cc1f0f93750f6d73b8de41a3e5daac7d8d61f88aa8f998db9a70b320a5df8b52");
        }

        private static void Case_05125()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5125,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-15,20,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,8,38,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,8,82,30,2), new GeneratedEnemyUnit(8,-18,53,31,3), new GeneratedEnemyUnit(-2,2,59,26,3), new GeneratedEnemyUnit(8,15,54,1,1), new GeneratedEnemyUnit(8,20,72,3,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "88a52a93e8990291dc69a8371ddc58f41b1bffd5da11936018b496ec7eda54bf");
        }

        private static void Case_05126()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5126,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-13,52,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-16,6,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,10,21,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-20,46,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-18,64,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-10,72,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-18,55,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-6,94,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-7,8,45,1), new GeneratedEnemyUnit(3,18,94,23,4), new GeneratedEnemyUnit(-6,8,81,3,3), new GeneratedEnemyUnit(10,-8,41,34,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "67793026ae7c1d0531756bc0bd0453be2340fcafb895dba0ab17a93f4754aa4e");
        }

        private static void Case_05127()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5127,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,7,38,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,3,55,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-18,91,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,3,70,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,1,53,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-17,66,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,2,82,16,1), new GeneratedEnemyUnit(11,14,71,18,2), new GeneratedEnemyUnit(-11,-4,43,20,1), new GeneratedEnemyUnit(14,1,38,4,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "5dc0153213f374f50c61268f2a49542cea0818e19880beff826c793130fc9c88");
        }

        private static void Case_05128()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5128,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,18,50,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,9,66,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-2,19,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-15,77,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-14,79,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,3,65,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,20,62,7,2), new GeneratedEnemyUnit(10,17,71,45,2), new GeneratedEnemyUnit(18,11,81,32,1), new GeneratedEnemyUnit(-14,-5,87,11,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "b9438105fda67bc699275f8609e047f1e7732e06450be75fe80a97698060baad");
        }

        private static void Case_05129()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5129,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-14,19,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-5,55,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-10,80,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-1,99,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-19,91,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,5,88,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,1,63,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-3,76,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,13,45,39,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "6bcca171d8c14b9be0bde6c8351e7341d99f5b331c35078f6af4bec9fadc8ac2");
        }

        private static void Case_05130()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5130,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,2,65,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,13,76,27,4), new GeneratedEnemyUnit(-10,0,64,39,4), new GeneratedEnemyUnit(-2,1,11,26,2), new GeneratedEnemyUnit(-12,14,38,8,1), new GeneratedEnemyUnit(20,17,89,41,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "aac988cd4a1759c39cfb9ed6c7a70394069179e7ae82c3741994d025b6f39c37");
        }

        private static void Case_05131()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5131,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-7,64,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,20,79,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-4,92,22,4), new GeneratedEnemyUnit(4,-19,82,47,4), new GeneratedEnemyUnit(-16,16,15,26,3), new GeneratedEnemyUnit(-1,18,40,10,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "bacd0db6c7ebbedf74a412b7c8219da527ca241f8906d2c1267dda39b5ed9aea");
        }

        private static void Case_05132()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5132,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,7,43,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,19,9,15,1), new GeneratedEnemyUnit(18,9,66,10,3), new GeneratedEnemyUnit(17,-6,75,32,4), new GeneratedEnemyUnit(-18,7,29,36,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "51015e759ecab392d24eb9fd08a3ce1f72924cb2e9635e7c96bed6a68889fca9");
        }

        private static void Case_05133()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5133,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-3,100,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-17,92,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,0,50,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-17,95,8,4), new GeneratedEnemyUnit(-19,10,39,16,4), new GeneratedEnemyUnit(7,5,31,38,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "3bc139bdfe95d105b0c9d1ac5d561db0dc17a43f259401799308d646df48fd9d");
        }

        private static void Case_05134()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5134,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,7,13,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-19,40,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-14,51,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-3,70,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-2,10,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-19,80,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,1,100,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,7,14,32,1), new GeneratedEnemyUnit(-17,-12,68,24,3), new GeneratedEnemyUnit(-6,-1,80,32,3), new GeneratedEnemyUnit(-12,18,77,10,2), new GeneratedEnemyUnit(-12,3,47,30,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "faf9cfd0e80f600aad4101fd33d8cafdd64469117550427034d2b14924c2993a");
        }

        private static void Case_05135()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5135,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-8,100,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-17,25,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,19,16,13,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "2db35a7087f65a1b4d07984ad10cebdcc64b5ec778a13ecfaa60208c9938b49c");
        }

        private static void Case_05136()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5136,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-20,73,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,1,44,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-7,90,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,12,92,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,2,50,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-8,38,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-16,83,31,2), new GeneratedEnemyUnit(2,-12,75,35,3), new GeneratedEnemyUnit(18,-20,26,15,3), new GeneratedEnemyUnit(4,-14,21,50,4), new GeneratedEnemyUnit(14,1,9,31,3), new GeneratedEnemyUnit(15,12,76,25,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "050ca7583170ce86fcc9761880fa191863940cfa160ff4dce0a676c95efb02c8");
        }

        private static void Case_05137()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5137,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-9,77,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-2,63,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,1,19,19,4), new GeneratedEnemyUnit(-12,-9,28,42,3), new GeneratedEnemyUnit(20,-4,83,35,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "f696c4078e16d9b7c3384d0f864756dc9b07f954ab973d876bbae786b4a733cd");
        }

        private static void Case_05138()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5138,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,5,31,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-18,33,46,2), new GeneratedEnemyUnit(19,11,17,29,1), new GeneratedEnemyUnit(-18,9,77,42,2), new GeneratedEnemyUnit(-12,7,55,36,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "bb3fd38dcccd0059eabcbb5581f23977e2ae97768ea81d983d4d454804ae184c");
        }

        private static void Case_05139()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5139,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-19,58,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-5,18,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,4,38,42,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "8290c83d3ce397c8df4f021f4057480dc1200c1af1972eb3ba120ad398f7c658");
        }

        private static void Case_05140()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5140,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,5,19,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-9,28,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-10,21,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-15,40,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,18,93,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,19,89,17,3), new GeneratedEnemyUnit(14,-15,16,10,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "8d245f97989799c66819636c4fa61ee93b8803fe787a50e36cf009186127bd7a");
        }

        private static void Case_05141()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5141,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,10,78,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-4,48,26,2), new GeneratedEnemyUnit(-3,13,98,41,4), new GeneratedEnemyUnit(-4,-17,40,45,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "a014feb1889710578ded41ea51d2eb8b7d10c3d5ad4d01e915522991f9651c66");
        }

        private static void Case_05142()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5142,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,19,28,4,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "e2c8f5d9ad422501d5fd8dcdbd1b747edfae0e434d384fd882784fecb0ca1cac");
        }

        private static void Case_05143()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5143,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,17,34,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,5,69,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,13,7,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,1,68,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,14,60,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,12,60,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-20,57,5,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "dd6010242290252188c838a3d29645da836e08add662cd3115bda6c80c9c7203");
        }

        private static void Case_05144()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5144,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-6,40,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,17,78,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,11,27,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,1,58,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-4,34,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-19,24,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-12,91,46,3), new GeneratedEnemyUnit(11,20,10,18,4), new GeneratedEnemyUnit(0,-4,11,27,1), new GeneratedEnemyUnit(8,-1,42,36,1), new GeneratedEnemyUnit(-10,2,59,39,4), new GeneratedEnemyUnit(20,-8,67,18,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "34268b580aad570bc89acad06a2235513d3de1ba9fe1571a5da125719a48756c");
        }

        private static void Case_05145()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5145,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,19,68,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,10,50,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,11,80,17,4), new GeneratedEnemyUnit(-3,-6,50,7,4), new GeneratedEnemyUnit(-13,17,88,45,4), new GeneratedEnemyUnit(-11,17,25,41,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "0f07bbf503fac86b76b188a9000392caa878b87f9465574322338cd17e0a68c8");
        }

        private static void Case_05146()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5146,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,2,78,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-4,30,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,4,11,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,14,68,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-18,62,5,3), new GeneratedEnemyUnit(13,17,50,39,2), new GeneratedEnemyUnit(-11,-15,36,18,4), new GeneratedEnemyUnit(-2,-4,100,47,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "e79a889494d12db605616387a1ff3673f8c03beb3e02bc5331f618ce5fc5de04");
        }

        private static void Case_05147()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5147,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,5,74,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-13,48,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-19,78,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-9,57,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-3,5,50,2), new GeneratedEnemyUnit(-9,20,83,35,3), new GeneratedEnemyUnit(-3,-18,79,2,3), new GeneratedEnemyUnit(-14,11,42,45,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "4d0ee7f9e37fce477c24df1caac3cf41f3fcf025566c1176e7789810387f9686");
        }

        private static void Case_05148()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5148,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-15,39,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,5,93,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-5,53,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,16,16,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-11,99,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-6,12,43,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "a50897a757046e8fceeebfc090ee0bb287784460ab42f90c84bc9afa22b1ca75");
        }

        private static void Case_05149()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5149,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-7,84,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,13,30,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-2,65,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,14,89,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-10,32,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,7,29,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,8,96,27,1), new GeneratedEnemyUnit(-16,20,65,33,3), new GeneratedEnemyUnit(-20,2,57,6,3), new GeneratedEnemyUnit(-8,16,75,30,1), new GeneratedEnemyUnit(-12,-14,28,33,1), new GeneratedEnemyUnit(9,13,92,3,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "c58dd176f810544f3792cf285a7aea96f96f3229c4efb96e43f8e7f668e95936");
        }

        private static void Case_05150()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5150,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-19,57,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,9,12,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,20,72,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,3,28,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-2,78,39,3), new GeneratedEnemyUnit(16,-11,98,7,3), new GeneratedEnemyUnit(18,-19,67,8,1), new GeneratedEnemyUnit(6,1,91,31,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "fed12b88910cf66a7d4fcac445aaca15b06c09c29d6d65ff9881118dda0ddde8");
        }

        private static void Case_05151()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5151,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-6,50,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,20,56,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,7,94,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-20,94,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,15,67,1,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "545b74dc775784564d7e62711d7b20bc3ce8d4d272ecfd49c5f898f1e93079b6");
        }

        private static void Case_05152()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5152,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,0,32,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,18,49,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,8,92,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,17,91,25,4), new GeneratedEnemyUnit(17,4,29,43,3), new GeneratedEnemyUnit(-16,0,99,40,1), new GeneratedEnemyUnit(7,19,97,15,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "153ae8fb3ba4b91c27daa4636e0775f4e4dd227bd0c5a4c65ba1373d88140315");
        }

        private static void Case_05153()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5153,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,10,97,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,10,54,5,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "7350204cde1622a890989e9a7363856a85adc6ee7996320efc224d6893bf4604");
        }

        private static void Case_05154()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5154,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-12,85,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-19,98,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,3,22,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-7,47,18,3), new GeneratedEnemyUnit(14,-12,39,1,1), new GeneratedEnemyUnit(-5,-18,90,15,1), new GeneratedEnemyUnit(7,17,37,46,1), new GeneratedEnemyUnit(-6,-10,44,37,1), new GeneratedEnemyUnit(11,-7,33,28,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "a21169388c5ff135c58dc0e570f874d774629319cfdb9488e092478c0a5daeef");
        }

        private static void Case_05155()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5155,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-17,34,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,12,93,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,12,72,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-10,51,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,12,53,46,1), new GeneratedEnemyUnit(-1,6,76,19,2), new GeneratedEnemyUnit(0,12,21,12,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "06fa79276c4eaf19511d36014a0745642cd9b7f00f9ff2c1ce8bd2d459110f7a");
        }

        private static void Case_05156()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5156,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-19,29,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,16,43,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-7,10,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,2,79,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,20,69,50,1), new GeneratedEnemyUnit(-13,10,7,31,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "0e6f4f0b5dcbf122cb4ff8e8d2e805e79f21c40fee4db8fe08e167848e953baf");
        }

        private static void Case_05157()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5157,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,0,79,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,5,23,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-14,49,7,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-16,19,11,3), new GeneratedEnemyUnit(2,-12,90,42,2), new GeneratedEnemyUnit(11,3,63,47,3), new GeneratedEnemyUnit(9,11,39,20,1), new GeneratedEnemyUnit(5,4,31,31,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "f1cf7975248e7b5c30e2efa4161d3e09efa9b85cb672e89cfc71976ec00743f2");
        }

        private static void Case_05158()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5158,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-16,55,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-1,42,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-14,94,22,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "11759f8833979216a7ca7be85b45311387744b59cb1c04bc3ced96be8c9b5564");
        }

        private static void Case_05159()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5159,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-10,23,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-9,51,6,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-13,76,7,1), new GeneratedEnemyUnit(1,-18,50,7,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "75d6472e5a84ea712ef9e3abd0b53b50369e52f9f467bf19650623e621027154");
        }

        private static void Case_05160()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5160,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-10,80,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-14,64,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-15,48,26,1), new GeneratedEnemyUnit(4,-20,66,21,1), new GeneratedEnemyUnit(7,11,14,26,1), new GeneratedEnemyUnit(-3,-5,91,43,4), new GeneratedEnemyUnit(6,6,67,18,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "b8d3ad3fb4628a932749fce1547e50976e709c7379b4f074492e141e13aa4f01");
        }

        private static void Case_05161()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5161,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,19,45,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,13,87,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,10,85,1,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "488154efd1927697a1afdd8a869a12fdaec287493bf2d5cf9a05533adecd4394");
        }

        private static void Case_05162()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5162,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-17,10,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-20,79,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,6,59,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-11,63,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,14,83,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,7,43,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,16,31,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-13,31,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-3,36,21,1), new GeneratedEnemyUnit(-11,-18,82,25,3), new GeneratedEnemyUnit(10,18,19,5,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "beb892dcde55265ab2f69c02547765bf4515e209517d005d6202bd2ce8b3cd52");
        }

        private static void Case_05163()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5163,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-7,60,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,2,86,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,6,69,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,13,9,19,1), new GeneratedEnemyUnit(5,-8,27,21,3), new GeneratedEnemyUnit(4,-13,89,18,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "7ec8f7bfddba0fee9228965fb0f393fb1a326a46175a2a99159537b2ae18bf26");
        }

        private static void Case_05164()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5164,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-12,6,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-7,22,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,19,87,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,2,35,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,10,64,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-19,59,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,0,46,49,2), new GeneratedEnemyUnit(5,-3,10,22,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "60ec029f3549ce00d2cec8e786242ad811e9e3c7a1953c9cff645f8e9114fa04");
        }

        private static void Case_05165()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5165,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,12,28,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,18,23,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,8,64,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,8,28,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,3,47,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,15,89,7,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "f4c75194f7ee75f2336a90f87c64b967a0f87598137c39700029614282a33239");
        }

        private static void Case_05166()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5166,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,5,83,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,18,84,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,3,49,6,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,8,32,36,1), new GeneratedEnemyUnit(-15,-4,56,50,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "95ab241edb644897dc5402cdb20016c4ecd8c47c59b5b23d4250630bb6c8d56b");
        }

        private static void Case_05167()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5167,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-4,79,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-15,35,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-14,30,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-7,89,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-15,76,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-7,14,34,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 75,
                stableHash: "b261f4236288ae5bfccd5898746148844010b85b6403f37aaaed8e8bb29e70f9");
        }

        private static void Case_05168()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5168,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,6,63,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-19,98,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,4,61,30,3), new GeneratedEnemyUnit(20,-8,51,17,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "203fc038ab1e185312e30b6d720c1fa3729312ce14ff3b91525c100beb54f02e");
        }

        private static void Case_05169()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5169,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-10,40,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,12,58,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-12,13,7,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "710e9e06a812096c64f0c5e9f560a0ee80644a9147edaf3e58369a49abf6b373");
        }

        private static void Case_05170()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5170,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,9,90,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,19,44,31,2), new GeneratedEnemyUnit(-9,13,43,50,4), new GeneratedEnemyUnit(-19,-9,86,44,1), new GeneratedEnemyUnit(20,11,57,45,2), new GeneratedEnemyUnit(13,-5,56,10,1), new GeneratedEnemyUnit(17,13,50,42,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "3a3340e91399be76ed15e4e066e7cb5fb82106b58d5ad9d3ebfd3a7a290dc0b3");
        }

        private static void Case_05171()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5171,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,10,28,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,15,45,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-9,86,26,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "d0ad3d82ae53940c87bbd09b650187bbe37436a9e3150c452570262697f17b28");
        }

        private static void Case_05172()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5172,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,7,21,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,1,67,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,10,86,42,4), new GeneratedEnemyUnit(15,8,99,4,2), new GeneratedEnemyUnit(13,-11,82,13,1), new GeneratedEnemyUnit(-11,17,76,31,4), new GeneratedEnemyUnit(-5,19,46,11,4), new GeneratedEnemyUnit(17,7,72,19,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "c5c55a6c5782e568e8637c9a095ea1fd5487797584298a293d8cb5346f65f94f");
        }

        private static void Case_05173()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5173,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,20,76,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-10,44,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-14,78,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,20,56,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,8,79,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,3,40,10,1), new GeneratedEnemyUnit(-18,-8,13,48,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "f2a0cfe19516065ebc16966bd8264bafc7c0ee5ff7ec11229c127ca04c6e9110");
        }

        private static void Case_05174()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5174,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,7,86,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,9,24,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-15,40,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,12,54,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,8,78,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-15,71,1,1), new GeneratedEnemyUnit(4,3,30,36,2), new GeneratedEnemyUnit(14,-5,46,33,4), new GeneratedEnemyUnit(2,13,94,46,3), new GeneratedEnemyUnit(5,11,76,10,1), new GeneratedEnemyUnit(-15,0,91,12,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "7d5ee593b909b8322c2394299de24c5dc93a0f061f3623707ac99fdbdddff8b4");
        }

        private static void Case_05175()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5175,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-2,50,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,15,46,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-17,18,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-7,70,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-2,7,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,8,29,39,2), new GeneratedEnemyUnit(4,-18,44,40,4), new GeneratedEnemyUnit(-16,4,66,13,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "efb064486c8fd22e7dba6194bebe707501e68dffe6194605af588e2842ef5247");
        }

        private static void Case_05176()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5176,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,14,71,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-9,54,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-19,67,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-20,59,22,4), new GeneratedEnemyUnit(-12,-10,63,6,2), new GeneratedEnemyUnit(6,-4,67,8,3), new GeneratedEnemyUnit(15,-17,7,50,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "3b8d982817ee3a6f94ebc55fca697a39d4cef13946cf3b7d52f1b284133ff0dc");
        }

        private static void Case_05177()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5177,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-13,67,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-18,20,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-14,24,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-2,27,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-4,88,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,19,94,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,15,74,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-12,60,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-16,8,24,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "b5d726e413d5cb70dd1275716887d9fd34f9a68d0619c97d7aeb19f131647635");
        }

        private static void Case_05178()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5178,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-2,60,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,0,93,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-20,61,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,20,90,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,20,84,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,12,98,34,3), new GeneratedEnemyUnit(9,-9,74,5,4), new GeneratedEnemyUnit(-1,20,84,31,4), new GeneratedEnemyUnit(18,-16,33,22,1), new GeneratedEnemyUnit(2,-13,7,38,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "c7faf1e421a6f420989c1fa7fb0ae36be63efcd4847e6adcf319d9926e221358");
        }

        private static void Case_05179()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5179,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-16,24,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,9,98,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-18,34,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-19,83,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-11,16,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-14,15,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,18,19,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,17,36,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,15,30,47,4), new GeneratedEnemyUnit(19,-17,50,30,2), new GeneratedEnemyUnit(11,5,38,32,4), new GeneratedEnemyUnit(12,20,77,43,2), new GeneratedEnemyUnit(19,-9,34,15,2), new GeneratedEnemyUnit(11,-4,61,3,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "281c7ab86536f93fe4a2b3306c68af47d1b7a25ca4c0e62d44b21db04425e1ee");
        }

        private static void Case_05180()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5180,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,14,61,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,17,80,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,20,38,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,19,96,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,8,76,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,5,43,4,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "f7c513dc9db36e91488571cd566a52631c0ff0a5d7760637311e7a0d921682a4");
        }

        private static void Case_05181()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5181,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-7,64,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-10,45,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,19,78,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,7,7,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-8,5,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-16,24,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,4,96,11,2), new GeneratedEnemyUnit(-18,14,89,30,1), new GeneratedEnemyUnit(19,4,24,20,2), new GeneratedEnemyUnit(11,-17,73,36,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "d5282393d1c92332d1e4b8238e015cba21af79d72ef02e3d266bfcea433db1c7");
        }

        private static void Case_05182()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5182,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-10,59,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,2,94,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,16,13,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-10,50,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-9,25,49,3), new GeneratedEnemyUnit(17,-19,45,5,3), new GeneratedEnemyUnit(9,-7,47,35,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "7b680cb511098ec6464388a5e5f00c04e095987ed14624f994f853f7c2621bb7");
        }

        private static void Case_05183()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5183,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-10,51,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-5,40,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,8,19,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-4,69,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,10,43,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-13,22,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,2,61,47,2), new GeneratedEnemyUnit(19,-2,41,14,3), new GeneratedEnemyUnit(18,-10,58,25,2), new GeneratedEnemyUnit(3,-19,69,49,2), new GeneratedEnemyUnit(0,0,72,19,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "8cb7c3531d6af16900dee64acfbd4b4787c500edd5ad35e629f741b634cb424e");
        }

        private static void Case_05184()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5184,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-9,57,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-1,48,6,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-12,32,7,4), new GeneratedEnemyUnit(10,12,9,11,1), new GeneratedEnemyUnit(-2,11,27,39,4), new GeneratedEnemyUnit(-6,-16,12,27,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "e3f5da996d8aac9eb4e5651a6d2a233abca872e6813b99643d4a4200170d1045");
        }

        private static void Case_05185()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5185,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-2,89,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-4,92,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-11,31,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-17,6,5,3), new GeneratedEnemyUnit(20,4,21,37,1), new GeneratedEnemyUnit(3,-6,60,42,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 30,
                stableHash: "0e6c1c968ac83235e92cc290b02756cd55ee50ad9caf900d3d3246599bbded84");
        }

        private static void Case_05186()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5186,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,14,12,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,0,87,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,17,28,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-2,16,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,17,86,3,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "71405c81464be38e257b4cc652fb442ffc5117d4a9e1c6c7d0c23576c97b1dbb");
        }

        private static void Case_05187()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5187,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,18,88,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,12,70,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-6,10,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-3,44,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-10,9,7,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "6e3cf1520afbd73a69674d70cbf04f78d613a17762330b55aad6239487b6143a");
        }

        private static void Case_05188()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5188,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,5,76,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-8,33,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-8,78,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-14,54,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,4,66,6,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "78275a710bda8e977e7f082bc6d70468c256c0a8219b64c29174c9c6f9da4af0");
        }

        private static void Case_05189()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5189,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-10,74,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-8,63,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-1,21,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,4,5,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,1,68,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,5,96,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,2,40,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,12,53,3,4), new GeneratedEnemyUnit(6,-11,13,37,2), new GeneratedEnemyUnit(-16,20,58,13,3), new GeneratedEnemyUnit(4,-7,34,34,4), new GeneratedEnemyUnit(18,-9,40,15,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "de069fff3121af06ac384ce6fce098713cad60c82aeb5fd77f12443d17fbe382");
        }

        private static void Case_05190()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5190,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,4,17,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-11,47,2,3), new GeneratedEnemyUnit(19,10,36,50,4), new GeneratedEnemyUnit(13,-13,70,33,1), new GeneratedEnemyUnit(20,0,80,17,1), new GeneratedEnemyUnit(-6,17,17,9,3), new GeneratedEnemyUnit(8,1,66,28,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "6b7ad51fcfc5fe0227e16230f0ff7e9485d82229e0e4f5f8240d41d76987f5a0");
        }

        private static void Case_05191()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5191,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-13,72,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-2,92,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-7,61,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,5,24,13,3), new GeneratedEnemyUnit(-5,10,94,28,3), new GeneratedEnemyUnit(11,18,79,19,2), new GeneratedEnemyUnit(-18,-14,10,40,2), new GeneratedEnemyUnit(15,15,99,13,4), new GeneratedEnemyUnit(-1,6,36,5,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "eee33772c5a181e8403a647f43a8c68e94a99ae57b87755eda644995820d3115");
        }

        private static void Case_05192()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5192,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,7,24,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,8,30,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,8,93,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,17,88,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-4,50,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,1,93,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-16,55,8,4), new GeneratedEnemyUnit(8,16,22,33,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "c90bf36d339f439ca8caf4836d27ad9b8151fc58faa940b4d2b4d03462928167");
        }

        private static void Case_05193()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5193,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,9,48,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-10,39,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-11,40,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-12,89,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-17,10,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,12,89,1,4), new GeneratedEnemyUnit(3,-5,34,44,1), new GeneratedEnemyUnit(2,17,46,33,2), new GeneratedEnemyUnit(-20,-18,15,2,4), new GeneratedEnemyUnit(-19,4,18,45,1), new GeneratedEnemyUnit(19,-6,98,43,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "09bbe02c64dcc1ffddc0840dfe323441285e59eaeec4ea711ab7eadb92296da8");
        }

        private static void Case_05194()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5194,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-8,65,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,4,15,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,0,20,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-13,84,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,12,66,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,7,24,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-2,54,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-5,54,35,3), new GeneratedEnemyUnit(1,5,47,28,3), new GeneratedEnemyUnit(-20,13,32,11,3), new GeneratedEnemyUnit(4,10,67,19,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "646e4b89ba3348eff9b0a9add77a47b1453efe202cc1ca14052d1318493519bd");
        }

        private static void Case_05195()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5195,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,13,31,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,7,31,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,3,51,6,1), new GeneratedEnemyUnit(13,-4,69,42,4), new GeneratedEnemyUnit(-7,-7,33,6,4), new GeneratedEnemyUnit(-1,-12,56,49,1), new GeneratedEnemyUnit(-13,-10,16,34,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "f7085ebd96affee4200a8e91b9734aab86c085483dde618eb4180df409350768");
        }

        private static void Case_05196()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5196,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,17,29,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-1,43,42,1), new GeneratedEnemyUnit(-20,-2,29,7,1), new GeneratedEnemyUnit(-3,-8,38,25,3), new GeneratedEnemyUnit(12,-10,80,44,2), new GeneratedEnemyUnit(-14,-13,90,42,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "38d0bbfd406c4e2b20c0208f6517cd943928e6aa20c22bfe64c79dc71b94ac68");
        }

        private static void Case_05197()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5197,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,18,96,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,0,90,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,18,91,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-19,41,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-4,49,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-14,51,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,3,37,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,2,89,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,11,46,9,4), new GeneratedEnemyUnit(-4,20,31,4,1), new GeneratedEnemyUnit(-2,0,28,25,1), new GeneratedEnemyUnit(6,11,20,11,4), new GeneratedEnemyUnit(11,11,23,43,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 55,
                stableHash: "aa931edc8001db87ab6e91661ab29939677435e14e911f56a3776bd7b235bb42");
        }

        private static void Case_05198()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5198,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-14,62,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,6,12,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,10,14,38,2), new GeneratedEnemyUnit(-5,4,14,17,1), new GeneratedEnemyUnit(5,-20,24,48,4), new GeneratedEnemyUnit(15,17,89,45,2), new GeneratedEnemyUnit(4,-17,39,21,4), new GeneratedEnemyUnit(19,7,89,31,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "ecf515096a5822099b0ed9fc384ac0009fe4c421892d4491a15437f92f64d2ad");
        }

        private static void Case_05199()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5199,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-3,34,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-6,8,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-17,42,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,8,61,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-9,60,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-14,24,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,1,57,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,1,94,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,17,22,9,4), new GeneratedEnemyUnit(10,9,70,6,1), new GeneratedEnemyUnit(-14,-16,77,15,1), new GeneratedEnemyUnit(0,-6,12,38,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "cb9c6967af867f6bed767826cb0e7a7f4955b0e81f5fa41318bcb5d1ee1f2c51");
        }

        private static void Case_05200()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5200,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-16,34,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,11,48,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,4,34,50,4), new GeneratedEnemyUnit(-6,19,46,46,4), new GeneratedEnemyUnit(19,-16,62,34,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "e98c356ac74e213a91f9fd6a534b36c6e2f9364c9c6252bb9381d33029a49d74");
        }

        private static void Case_05201()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5201,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,11,18,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-20,82,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,0,21,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-2,68,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,14,20,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,14,58,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,17,69,4,4), new GeneratedEnemyUnit(-4,17,73,40,4), new GeneratedEnemyUnit(9,-13,52,35,1), new GeneratedEnemyUnit(12,-20,63,41,2), new GeneratedEnemyUnit(-10,-9,84,50,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "84ce475b9005d1e506ac1302345529051a9e3e2e4d8bf7825c987d895b19186e");
        }

        private static void Case_05202()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5202,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,2,23,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,3,26,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-15,48,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,7,92,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-6,56,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,15,87,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,3,9,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-17,74,7,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "385e0297dd2b421252bba1893cf88b5a89b510c8a64f2af1eb0bcc860012112f");
        }

        private static void Case_05203()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5203,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,1,15,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,7,6,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,14,96,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,13,74,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,11,32,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-17,72,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,16,66,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,15,42,5,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "b6b3818243c4dfbdd248e7a3893d55fbf8868af4d1e9253090386095c50d08bb");
        }

        private static void Case_05204()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5204,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,19,49,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,5,9,32,1), new GeneratedEnemyUnit(-10,19,66,20,1), new GeneratedEnemyUnit(6,3,72,5,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "1a80440d169f9c83db5e9ab4e47aa3ff3d2d2160a3315290f14ef4cd9176b808");
        }

        private static void Case_05205()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5205,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,20,100,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,20,21,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,3,20,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-20,38,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,7,23,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,2,52,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-20,65,3,4), new GeneratedEnemyUnit(6,-7,100,22,4), new GeneratedEnemyUnit(3,-12,30,19,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "1cbf480da92bad0de8efca378e340ddaf2f91d7a0de6b34bd202b5b1f032d60b");
        }

        private static void Case_05206()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5206,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,3,31,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-8,57,13,1), new GeneratedEnemyUnit(9,0,46,16,2), new GeneratedEnemyUnit(-20,-18,93,8,1), new GeneratedEnemyUnit(-7,16,85,9,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "4801d6de662eb3769ce7317f03673bcb63b67cefcb50139c409a0b00451efe10");
        }

        private static void Case_05207()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5207,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,15,53,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-9,60,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,0,46,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-18,17,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-15,38,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-1,29,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,0,15,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,4,10,6,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "c3933e60c30b27c7aa45a6ddb7b84931098d5f4598f5c79b89cc5369a955f79a");
        }

        private static void Case_05208()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5208,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-5,45,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,8,54,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,11,73,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-16,82,46,4), new GeneratedEnemyUnit(8,-11,33,46,4), new GeneratedEnemyUnit(-12,-8,12,10,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "860eb8cb9acfd3889ec06a0fcd0862f682d4e49279567fc9fd06bde3038b9218");
        }

        private static void Case_05209()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5209,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-9,17,7,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "8e2812935375f7265b7132822a681e4b708ed7b5c580519812b3aaecd6909f49");
        }

        private static void Case_05210()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5210,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-3,9,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,4,72,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,19,51,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,11,79,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-15,69,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-16,67,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,6,41,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,7,22,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-18,95,28,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 75,
                stableHash: "becbdfa4c725b2d3e5d622907548f284a3c7ecd6baf23226cbf8cd230acd4e4a");
        }

        private static void Case_05211()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5211,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-20,62,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-11,82,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-17,51,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,15,42,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,5,23,1,4), new GeneratedEnemyUnit(-6,8,68,16,4), new GeneratedEnemyUnit(-17,11,77,31,1), new GeneratedEnemyUnit(19,16,52,30,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "aaf86e93e511d42943bdfd70631ef5219911fc25dcdef7edee1104577bb09473");
        }

        private static void Case_05212()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5212,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-4,78,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,15,48,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-18,83,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-9,27,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,7,21,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-2,68,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,4,38,5,4), new GeneratedEnemyUnit(-3,4,49,32,2), new GeneratedEnemyUnit(-17,-12,28,24,2), new GeneratedEnemyUnit(-8,-7,69,5,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "0c39153e0d511c6a254a457664fc365707efde3d0cdfa2150fe0ce9c69230da9");
        }

        private static void Case_05213()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5213,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-11,54,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-11,81,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-19,28,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-11,56,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,12,88,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,1,43,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,11,52,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,11,78,7,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "b8bfa39e53b2311147153a9d466fdce3f194f7b3ce67e7b19671a021e3348a6e");
        }

        private static void Case_05214()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5214,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-11,89,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,11,34,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,14,23,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,5,35,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,10,24,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-7,6,12,2), new GeneratedEnemyUnit(10,10,31,33,4), new GeneratedEnemyUnit(-6,13,14,44,2), new GeneratedEnemyUnit(-19,14,87,37,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "577cf37e9b064ee66cee667a47b30b6a362338a06f2ea209ffe390f6d53c451c");
        }

        private static void Case_05215()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5215,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,13,29,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,1,53,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,12,27,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-1,19,40,2), new GeneratedEnemyUnit(-3,-17,98,33,2), new GeneratedEnemyUnit(-14,-4,11,23,2), new GeneratedEnemyUnit(-6,9,5,30,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "6f95dd642bc2b71ebf65b96e0b9770f80340569c69c698964847c0c16878ae05");
        }

        private static void Case_05216()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5216,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,1,57,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,14,6,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,13,56,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,6,12,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,6,47,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,13,35,44,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "d742ece200b2b0b50f74ce005cfd13e7f79bc31fe9268d2648477b55daf7bb1a");
        }

        private static void Case_05217()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5217,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,15,51,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,13,48,5,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-15,8,6,2), new GeneratedEnemyUnit(-3,1,31,12,1), new GeneratedEnemyUnit(7,6,60,8,1), new GeneratedEnemyUnit(-17,2,78,41,4), new GeneratedEnemyUnit(13,10,54,13,1), new GeneratedEnemyUnit(-3,16,11,5,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "b2b0e1e2abbc2b2fe4dbe2c929c3fad0842f2bcd8ad4b00aac8b65ad55205ee3");
        }

        private static void Case_05218()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5218,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,0,20,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-17,18,44,3), new GeneratedEnemyUnit(0,-8,91,11,2), new GeneratedEnemyUnit(16,16,7,29,1), new GeneratedEnemyUnit(3,5,47,47,4), new GeneratedEnemyUnit(-7,3,79,40,2), new GeneratedEnemyUnit(-19,2,43,3,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "15a427632775302cd9aca6cd0025a0e2520df1fa3d3e97b3459ef7209dcf9b27");
        }

        private static void Case_05219()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 5219,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,19,18,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-6,85,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,18,57,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-13,12,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,10,18,37,3), new GeneratedEnemyUnit(16,8,5,21,4), new GeneratedEnemyUnit(12,-10,22,25,2), new GeneratedEnemyUnit(-3,-15,90,1,3), new GeneratedEnemyUnit(5,0,48,42,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "3b1b223c70c4e48732d4e33344d6f52f902ad5da0284033919a925ff0af6e288");
        }

    }
}
