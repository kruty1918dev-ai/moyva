using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard005
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_00900();
            Case_00901();
            Case_00902();
            Case_00903();
            Case_00904();
            Case_00905();
            Case_00906();
            Case_00907();
            Case_00908();
            Case_00909();
            Case_00910();
            Case_00911();
            Case_00912();
            Case_00913();
            Case_00914();
            Case_00915();
            Case_00916();
            Case_00917();
            Case_00918();
            Case_00919();
            Case_00920();
            Case_00921();
            Case_00922();
            Case_00923();
            Case_00924();
            Case_00925();
            Case_00926();
            Case_00927();
            Case_00928();
            Case_00929();
            Case_00930();
            Case_00931();
            Case_00932();
            Case_00933();
            Case_00934();
            Case_00935();
            Case_00936();
            Case_00937();
            Case_00938();
            Case_00939();
            Case_00940();
            Case_00941();
            Case_00942();
            Case_00943();
            Case_00944();
            Case_00945();
            Case_00946();
            Case_00947();
            Case_00948();
            Case_00949();
            Case_00950();
            Case_00951();
            Case_00952();
            Case_00953();
            Case_00954();
            Case_00955();
            Case_00956();
            Case_00957();
            Case_00958();
            Case_00959();
            Case_00960();
            Case_00961();
            Case_00962();
            Case_00963();
            Case_00964();
            Case_00965();
            Case_00966();
            Case_00967();
            Case_00968();
            Case_00969();
            Case_00970();
            Case_00971();
            Case_00972();
            Case_00973();
            Case_00974();
            Case_00975();
            Case_00976();
            Case_00977();
            Case_00978();
            Case_00979();
            Case_00980();
            Case_00981();
            Case_00982();
            Case_00983();
            Case_00984();
            Case_00985();
            Case_00986();
            Case_00987();
            Case_00988();
            Case_00989();
            Case_00990();
            Case_00991();
            Case_00992();
            Case_00993();
            Case_00994();
            Case_00995();
            Case_00996();
            Case_00997();
            Case_00998();
            Case_00999();
            Case_01000();
            Case_01001();
            Case_01002();
            Case_01003();
            Case_01004();
            Case_01005();
            Case_01006();
            Case_01007();
            Case_01008();
            Case_01009();
            Case_01010();
            Case_01011();
            Case_01012();
            Case_01013();
            Case_01014();
            Case_01015();
            Case_01016();
            Case_01017();
            Case_01018();
            Case_01019();
            Case_01020();
            Case_01021();
            Case_01022();
            Case_01023();
            Case_01024();
            Case_01025();
            Case_01026();
            Case_01027();
            Case_01028();
            Case_01029();
            Case_01030();
            Case_01031();
            Case_01032();
            Case_01033();
            Case_01034();
            Case_01035();
            Case_01036();
            Case_01037();
            Case_01038();
            Case_01039();
            Case_01040();
            Case_01041();
            Case_01042();
            Case_01043();
            Case_01044();
            Case_01045();
            Case_01046();
            Case_01047();
            Case_01048();
            Case_01049();
            Case_01050();
            Case_01051();
            Case_01052();
            Case_01053();
            Case_01054();
            Case_01055();
            Case_01056();
            Case_01057();
            Case_01058();
            Case_01059();
            Case_01060();
            Case_01061();
            Case_01062();
            Case_01063();
            Case_01064();
            Case_01065();
            Case_01066();
            Case_01067();
            Case_01068();
            Case_01069();
            Case_01070();
            Case_01071();
            Case_01072();
            Case_01073();
            Case_01074();
            Case_01075();
            Case_01076();
            Case_01077();
            Case_01078();
            Case_01079();
        }

        private static void Case_00900()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 900,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-16,68,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,19,34,45,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "d21f37c4877623c507f12027a90eac2ca9b135fb4991d5a3f2093a957ffcaba8");
        }

        private static void Case_00901()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 901,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,1,86,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-13,71,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,20,12,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-17,30,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-10,61,41,3), new GeneratedEnemyUnit(-15,-2,21,40,3), new GeneratedEnemyUnit(14,15,50,18,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "5e21f5eb1c73554ecdb82e845096d405e2da7e04bc80a382e541be0da9b9e3d3");
        }

        private static void Case_00902()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 902,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-12,34,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,2,41,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,19,97,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,0,40,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,10,96,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,14,92,5,2), new GeneratedEnemyUnit(-13,-9,27,35,1), new GeneratedEnemyUnit(-11,-10,57,8,2), new GeneratedEnemyUnit(4,1,19,37,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "c1677444d6644ac190565c549e8b47e4529033bb719e97fedd6523e93ce082ba");
        }

        private static void Case_00903()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 903,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-14,44,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-19,98,5,4), new GeneratedEnemyUnit(-12,1,92,3,1), new GeneratedEnemyUnit(-3,1,47,48,2), new GeneratedEnemyUnit(-1,3,68,18,3), new GeneratedEnemyUnit(-7,16,95,25,1), new GeneratedEnemyUnit(-15,12,64,2,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "603d71580716e477bbab016feb41a86a64e04e64f33af1124c246a8e15e8954c");
        }

        private static void Case_00904()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 904,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,17,91,4,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,15,6,44,4), new GeneratedEnemyUnit(-16,-16,87,22,4), new GeneratedEnemyUnit(8,-3,79,24,4), new GeneratedEnemyUnit(6,14,41,29,4), new GeneratedEnemyUnit(0,9,40,37,4), new GeneratedEnemyUnit(-18,-10,16,38,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "f2e5756ace5b17e40d747dfa9126c9b35645f5ab1b523b5ecf3dab7fe89e0e1f");
        }

        private static void Case_00905()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 905,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,7,86,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,11,90,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,14,35,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,19,51,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,13,36,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,11,86,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,6,92,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-8,90,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,10,44,32,4), new GeneratedEnemyUnit(16,2,44,48,1), new GeneratedEnemyUnit(-13,11,37,20,1), new GeneratedEnemyUnit(-8,6,56,31,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "fcb86971e53691c3cdf02c75f3cea1aff75ed2094d8e98580c4874499ca6686f");
        }

        private static void Case_00906()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 906,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,18,14,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-16,29,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,16,63,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,9,53,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-20,34,43,1), new GeneratedEnemyUnit(1,4,87,31,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "b2e8ff1c703b1077b9961630fbea1dc54f6441027ffb457d1cd323925716014f");
        }

        private static void Case_00907()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 907,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-19,46,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,19,77,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-11,25,36,1), new GeneratedEnemyUnit(3,19,37,13,4), new GeneratedEnemyUnit(11,-15,72,19,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "e56a2dffe909b5423f31bfeef220d0b49a8da0bc14e4733fd5f801d68dde53c5");
        }

        private static void Case_00908()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 908,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-6,45,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-4,82,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-17,67,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-9,66,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-19,19,49,2), new GeneratedEnemyUnit(-20,-11,96,1,3), new GeneratedEnemyUnit(-18,19,83,35,1), new GeneratedEnemyUnit(7,14,50,33,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "4b017929dabf853ebad2c6b35a7a6bd241fe00743eade88d3b72cb5059956fd1");
        }

        private static void Case_00909()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 909,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,5,21,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,14,35,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-18,79,8,4), new GeneratedEnemyUnit(10,19,45,23,3), new GeneratedEnemyUnit(-15,-1,57,40,4), new GeneratedEnemyUnit(-18,2,65,23,2), new GeneratedEnemyUnit(-4,2,12,30,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 55,
                stableHash: "a12f5becb7c9f968b5005a4a9172a560bd0d9071f1f12edf285405c7acbe1957");
        }

        private static void Case_00910()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 910,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,8,5,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,12,90,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-4,38,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,19,61,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-12,24,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,11,85,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,4,31,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-3,53,5,3), new GeneratedEnemyUnit(18,20,81,30,1), new GeneratedEnemyUnit(-14,8,85,43,1), new GeneratedEnemyUnit(11,0,76,23,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "d8ad9c3464ca9b0b8112d767e05b73b2c0cf7f339abffcf2cdb684bbe54a8188");
        }

        private static void Case_00911()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 911,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,11,48,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-11,85,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,3,46,29,4), new GeneratedEnemyUnit(-5,9,39,49,2), new GeneratedEnemyUnit(-6,-8,87,30,2), new GeneratedEnemyUnit(5,-16,43,1,1), new GeneratedEnemyUnit(15,-9,26,29,1), new GeneratedEnemyUnit(-19,-20,94,15,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "d5d23cdb76eae32c57b6d391639c37dda43170bd1945b8ce38d13fe1f6ce0182");
        }

        private static void Case_00912()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 912,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,20,65,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,11,36,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,0,80,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,18,56,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,13,61,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-7,97,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-4,53,5,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "217e0661acfcf6b672f11a9fe50d659aec3e54bc49409a687ed9a6ee751e6bf5");
        }

        private static void Case_00913()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 913,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-19,78,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-4,26,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-8,9,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,14,66,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,16,65,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-11,64,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,2,85,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-1,87,2,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "33061eb3dab1c3e852f932cab6ff4fd9123383b4b46d528913bf775adcf13c26");
        }

        private static void Case_00914()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 914,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-14,80,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,1,11,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,20,69,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,3,67,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,2,24,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-16,81,2,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-15,82,10,4), new GeneratedEnemyUnit(-14,-17,72,17,2), new GeneratedEnemyUnit(7,15,87,3,1), new GeneratedEnemyUnit(5,0,62,46,2), new GeneratedEnemyUnit(6,15,23,28,1), new GeneratedEnemyUnit(-10,-4,76,32,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "50b1bc90b78aa48c35f3e90253301cd68f418e9ad9e6cbbd8957ef47666ff5c1");
        }

        private static void Case_00915()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 915,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,13,83,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-12,88,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-17,33,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,7,56,39,4), new GeneratedEnemyUnit(9,-1,28,50,4), new GeneratedEnemyUnit(2,13,23,49,1), new GeneratedEnemyUnit(-13,7,20,34,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "2f791bde59d029a73271ca99d9bdb0c998ed81c69cce3431522ef3ae0e0c7328");
        }

        private static void Case_00916()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 916,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-10,33,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-2,43,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,2,46,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-15,44,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-17,46,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,0,28,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-9,65,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-9,97,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-9,91,10,4), new GeneratedEnemyUnit(13,3,5,44,3), new GeneratedEnemyUnit(17,-7,96,28,1), new GeneratedEnemyUnit(0,9,20,38,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "47c24089569e39cdf99dd7bf5a5761a2ac6780dc660cfacc5e67efaf43b67da3");
        }

        private static void Case_00917()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 917,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-10,91,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,10,70,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,12,6,32,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "7e2ced800d725eebcc5ef381d3cf1972053a74b7a6681b21d23ceffe64e696b2");
        }

        private static void Case_00918()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 918,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-13,77,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-1,82,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,1,94,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,4,92,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-17,32,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,3,13,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-5,35,32,4), new GeneratedEnemyUnit(-4,8,89,46,2), new GeneratedEnemyUnit(15,6,6,30,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "d934b369e9637617f62dd0b83ef77151396725958bd31de9c6f1454c9e1422cb");
        }

        private static void Case_00919()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 919,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,5,60,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-5,28,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-2,5,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-13,53,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,13,11,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-3,97,23,3), new GeneratedEnemyUnit(-12,14,97,49,1), new GeneratedEnemyUnit(-2,7,70,50,4), new GeneratedEnemyUnit(20,-7,74,17,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "e2a6b61391859bf789973825c04c6e8af7d9faae27a52213ae8fec7c11a66e99");
        }

        private static void Case_00920()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 920,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,17,65,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,10,21,22,4), new GeneratedEnemyUnit(-14,-2,94,25,4), new GeneratedEnemyUnit(0,14,59,46,1), new GeneratedEnemyUnit(2,-20,35,7,3), new GeneratedEnemyUnit(-18,-12,61,22,4), new GeneratedEnemyUnit(-18,3,24,5,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "fe8927157b7d5da7e637678f62dcd8c11dbf3474103028dd61adddf63d559389");
        }

        private static void Case_00921()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 921,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-6,86,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-7,74,49,3), new GeneratedEnemyUnit(1,-8,76,2,4), new GeneratedEnemyUnit(-6,19,15,3,3), new GeneratedEnemyUnit(-8,-12,93,19,4), new GeneratedEnemyUnit(-12,6,8,27,2), new GeneratedEnemyUnit(20,-7,100,6,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "c64e9ce113917eb0e1e26c2eb052351c69fb5b0b0fd60b7bd081fdbb59396508");
        }

        private static void Case_00922()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 922,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,2,73,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-2,70,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-9,25,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,0,16,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,7,9,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,12,42,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-9,72,33,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "01f42c62845034489390da03d0489ce6da76e7b0d2e83a50c77dc35c8e8658c1");
        }

        private static void Case_00923()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 923,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-16,42,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-13,93,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-3,25,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,5,45,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-14,32,48,4), new GeneratedEnemyUnit(-17,-6,57,14,1), new GeneratedEnemyUnit(-17,-9,80,11,2), new GeneratedEnemyUnit(12,1,24,48,3), new GeneratedEnemyUnit(-4,7,91,12,2), new GeneratedEnemyUnit(9,1,5,33,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "b26b4828e63069fb94a9f64e0aed5572f2b919d93693960109faf8105f483f2f");
        }

        private static void Case_00924()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 924,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-16,77,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-17,5,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-4,59,2,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "463b4503a51b8a3b2d5973e5579fc419e71a38e0dc415995f875cb4c81e8e813");
        }

        private static void Case_00925()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 925,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,17,23,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-1,20,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-13,28,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-4,25,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,14,50,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-10,45,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,17,93,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-11,57,7,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,2,62,39,3), new GeneratedEnemyUnit(-7,10,12,44,4), new GeneratedEnemyUnit(-8,20,9,1,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "fff7577b007f8981e0f0464ff6ffddf7bd1448ed764c58ac48992fb1cf8e2da1");
        }

        private static void Case_00926()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 926,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,11,27,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-16,54,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,13,11,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-19,95,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-10,50,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,18,30,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,1,54,1,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "f022b60a8db07645e222b80d13905df8f0d768ee681f725f881a50ba00683aaf");
        }

        private static void Case_00927()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 927,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-10,47,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,11,86,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,6,20,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-3,50,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,9,42,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,9,100,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,11,41,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-3,50,1,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "90678c1ffe0749e3afe03542e58b14c925c7bed5a08792c52fe7c25f642120b3");
        }

        private static void Case_00928()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 928,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,18,76,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,13,90,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-3,59,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-11,35,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,14,64,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-17,29,8,4), new GeneratedEnemyUnit(13,-18,40,3,4), new GeneratedEnemyUnit(-16,-12,21,38,3), new GeneratedEnemyUnit(20,-15,66,10,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "b6120ffcbc7a7505fcca48ebe1379c646a1690e59a0ca8b1e465fe7a7e1ffcc2");
        }

        private static void Case_00929()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 929,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,6,98,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-1,89,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-13,56,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,7,42,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,0,9,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,3,8,18,3), new GeneratedEnemyUnit(-19,-7,91,37,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "16638427b25bc8b3311f1a656286e68bc7161198f6f054b99de672acac96541b");
        }

        private static void Case_00930()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 930,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-5,63,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,10,78,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,7,33,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,3,60,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-19,81,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-3,5,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-2,12,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,16,92,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-20,64,9,1), new GeneratedEnemyUnit(-19,4,17,50,3), new GeneratedEnemyUnit(13,-2,14,19,2), new GeneratedEnemyUnit(16,-6,100,19,2), new GeneratedEnemyUnit(1,-10,64,7,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "ef66f8c7daea48e610025f0ea21d6b84306b7dfa7fba5102e92f8d0e7e9629b2");
        }

        private static void Case_00931()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 931,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-7,57,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-12,16,43,1), new GeneratedEnemyUnit(7,-2,35,30,4), new GeneratedEnemyUnit(15,6,10,8,2), new GeneratedEnemyUnit(-11,15,56,43,4), new GeneratedEnemyUnit(13,-8,44,2,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "51b54dee8d030be6c89cff269ad1582fb6271a6d8eac3bbec08dee43b6a5384e");
        }

        private static void Case_00932()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 932,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,2,41,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,10,21,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-16,73,50,2), new GeneratedEnemyUnit(-8,10,81,20,3), new GeneratedEnemyUnit(-17,-15,66,21,3), new GeneratedEnemyUnit(6,3,11,43,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "2c548713158de4f1977094a78d29623834626fdb8565c41e85aadde2b7dd45b1");
        }

        private static void Case_00933()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 933,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,6,94,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,8,30,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-19,98,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,16,32,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-15,93,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-12,55,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-11,6,7,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "b591eb087f2741c3355f53ae903b4826dc5aa2536e64ba850668bc6c7c141852");
        }

        private static void Case_00934()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 934,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,12,36,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,12,10,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-1,49,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,2,79,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-12,30,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-6,26,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-12,48,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,16,87,32,3), new GeneratedEnemyUnit(-6,8,71,18,2), new GeneratedEnemyUnit(14,-11,33,23,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "fd6fa7250626ef959d8859ac8a28e6139f8714062340dedf78bd076d9fd4528e");
        }

        private static void Case_00935()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 935,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-9,98,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-3,18,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,9,13,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-11,49,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-6,9,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-20,93,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,13,23,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-11,13,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-6,31,20,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 75,
                stableHash: "81f1aca5e6bb46bc5e2c06c5fdfca44daadfbd0d059bc211aad4ceb6ca143a08");
        }

        private static void Case_00936()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 936,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-10,73,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,12,71,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-1,85,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,3,63,17,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "cf61582410fe5e00c79b271fe157b18f8a0cb356e7387a5fbcff3c58f75c7447");
        }

        private static void Case_00937()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 937,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,1,67,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,12,55,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-4,14,21,1), new GeneratedEnemyUnit(-15,20,81,44,2), new GeneratedEnemyUnit(-13,-19,68,42,1), new GeneratedEnemyUnit(10,-6,100,14,3), new GeneratedEnemyUnit(-18,18,77,34,3), new GeneratedEnemyUnit(-2,19,25,15,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "e29073d80333124f76ab4e626ff019fb5eb9298297265990a6855c0e3847076a");
        }

        private static void Case_00938()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 938,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-6,97,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,10,25,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,6,90,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,15,18,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-5,51,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-12,67,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,17,6,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,4,24,2,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,7,58,47,3), new GeneratedEnemyUnit(-1,15,46,48,4), new GeneratedEnemyUnit(-4,1,75,36,2), new GeneratedEnemyUnit(-20,-13,11,44,1), new GeneratedEnemyUnit(-1,17,20,33,2), new GeneratedEnemyUnit(-10,-5,91,46,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "c1efddf72961f5401c5922e0850df01de29f080622842315f3f360d0320d41fe");
        }

        private static void Case_00939()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 939,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-9,87,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-19,73,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,19,60,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,15,46,2,2), new GeneratedEnemyUnit(9,-10,34,48,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "26cac158e50066452afdc5c6d5ace8d976cf762301ffd6d4759e83021dc60944");
        }

        private static void Case_00940()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 940,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,17,39,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-19,100,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,12,98,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,14,66,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,5,93,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-18,17,3,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "2b04dc1471cb470427bbaea7a0f1e164a055f07bf0cbda6d78ac5d6ee719300c");
        }

        private static void Case_00941()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 941,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-18,48,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-17,49,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-5,15,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-8,28,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,0,27,2,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,12,55,47,1), new GeneratedEnemyUnit(17,19,92,37,1), new GeneratedEnemyUnit(-5,14,70,31,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "0407efc92bb01827e9bfb3cfafefeaaabf2f1acfaf74d36834138b3fd7026d0d");
        }

        private static void Case_00942()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 942,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-18,54,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,5,50,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,19,62,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-18,5,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,14,40,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-2,94,38,4), new GeneratedEnemyUnit(-1,3,50,39,3), new GeneratedEnemyUnit(13,1,34,33,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "79dc4bfc7cd041e1b0eb81f27f44895b3e35d639c92d178d78a3ffedfc39927e");
        }

        private static void Case_00943()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 943,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,2,55,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-19,8,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-5,99,38,3), new GeneratedEnemyUnit(-18,9,43,3,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "59016db63b1ed8716dd52ba75212f32854787753cb4bef6c3ca3facb1210f71c");
        }

        private static void Case_00944()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 944,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,16,66,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,15,96,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,8,32,15,4), new GeneratedEnemyUnit(-8,12,77,6,4), new GeneratedEnemyUnit(-9,2,66,34,3), new GeneratedEnemyUnit(9,13,60,31,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "41efc5424f8d70ddccdae430eb4ec8b807e76915df47ad78d472ebd152d57a31");
        }

        private static void Case_00945()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 945,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,18,73,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,13,38,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,11,31,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-4,71,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-19,92,1,2), new GeneratedEnemyUnit(18,9,6,45,3), new GeneratedEnemyUnit(16,-5,37,42,3), new GeneratedEnemyUnit(-19,-8,75,36,4), new GeneratedEnemyUnit(9,20,15,9,2), new GeneratedEnemyUnit(-14,13,100,45,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "4a592484caee6d3458e39e7637baf7c8b229f67094b74a3320128047edc741b9");
        }

        private static void Case_00946()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 946,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,0,55,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,15,29,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,5,30,7,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,14,59,43,2), new GeneratedEnemyUnit(-2,-1,11,11,3), new GeneratedEnemyUnit(-6,-12,45,28,4), new GeneratedEnemyUnit(-11,-5,32,10,3), new GeneratedEnemyUnit(20,-18,27,3,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "7eb275d231ed8772d8546a9dc62feebea3b924efb04e0b294e048b82ebb0b3c6");
        }

        private static void Case_00947()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 947,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-12,14,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,20,59,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-12,12,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-4,88,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,13,9,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,14,17,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-13,24,22,1), new GeneratedEnemyUnit(-5,3,75,37,4), new GeneratedEnemyUnit(-4,12,93,24,1), new GeneratedEnemyUnit(-10,6,89,26,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "294566b4ba8e5fe766e75c1d98d52fd69b95123b16fd84ac621d12a2b819976d");
        }

        private static void Case_00948()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 948,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-7,64,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-19,6,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-7,36,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,0,47,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,13,11,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,16,92,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,1,42,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,1,24,1,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "184a4635b433281e067e6019b6e83398a118979e9fdc165889f250bc884d0fb4");
        }

        private static void Case_00949()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 949,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,1,96,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,3,55,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-8,43,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,8,30,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-8,6,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,7,87,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-7,21,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,20,80,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-19,59,13,2), new GeneratedEnemyUnit(13,14,45,45,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "ae511126899ac5bd97c881e9b454f6d2de93fe711a275a8c02ac4016b0a9827a");
        }

        private static void Case_00950()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 950,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,0,23,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,14,68,37,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "bc906bea40cac1dfde31de3c6881fab9e70e1419b028b85306132a684e3dd492");
        }

        private static void Case_00951()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 951,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,8,86,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-12,73,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,12,82,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,9,28,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,19,6,48,2), new GeneratedEnemyUnit(-20,-2,95,14,4), new GeneratedEnemyUnit(-2,-7,50,38,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "501cf927be42117b96a9ee9b7b2c9d050dff5a5d91adb626d1d566e22713b799");
        }

        private static void Case_00952()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 952,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,9,72,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-19,15,32,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "8a34911327626685f10dcde7e125e7dff1c646e43b5d6e849812badd80a91630");
        }

        private static void Case_00953()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 953,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,1,6,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-15,45,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,11,11,24,3), new GeneratedEnemyUnit(12,20,48,24,2), new GeneratedEnemyUnit(15,-8,21,30,1), new GeneratedEnemyUnit(-6,5,65,13,2), new GeneratedEnemyUnit(8,10,51,20,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "68ef39042a741f45527c7ca12c9a9ad0e318b3b426cd07cd673259b4410276c7");
        }

        private static void Case_00954()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 954,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-5,42,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,15,84,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,19,99,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,11,59,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-18,43,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-20,17,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,5,88,7,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,7,23,8,3), new GeneratedEnemyUnit(18,14,61,43,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "5230cb59912cd8a805907ea12face3e494da999af88e39c6e5a41d9f79090d00");
        }

        private static void Case_00955()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 955,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,12,54,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-19,95,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-12,59,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-17,85,27,2), new GeneratedEnemyUnit(-1,-17,46,3,1), new GeneratedEnemyUnit(-14,-3,7,35,1), new GeneratedEnemyUnit(-20,-18,5,20,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "38ae1209ce7c1890b624e7eba85c3cc23632789d0849eae2154b96b1199974ca");
        }

        private static void Case_00956()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 956,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,10,32,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,11,58,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,12,75,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,1,45,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-13,85,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,6,93,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,17,89,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-11,19,6,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "63e3210fe45880f04fb94dbdd32ff4e48f9ee5270acce7e03d5111ff5fcab7ff");
        }

        private static void Case_00957()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 957,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,11,39,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-15,100,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,19,72,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,5,17,11,3), new GeneratedEnemyUnit(16,10,9,13,3), new GeneratedEnemyUnit(-6,-8,39,42,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "499a9832510f7dd0c55c3811d7f84963ae9aac9c618d1bdc041ab343d7f3500c");
        }

        private static void Case_00958()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 958,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,16,82,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-15,20,13,1), new GeneratedEnemyUnit(14,14,30,49,2), new GeneratedEnemyUnit(10,-12,100,50,2), new GeneratedEnemyUnit(-3,-5,73,9,4), new GeneratedEnemyUnit(-3,5,39,41,3), new GeneratedEnemyUnit(-14,-14,61,13,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "751da4e2ac99e43a7a400e9b4bb19c893d8d01d89ccf704947c187b915291790");
        }

        private static void Case_00959()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 959,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-4,6,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,6,85,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,11,38,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-15,86,1,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "4656afa3918a892ecaebd95147a0a080c8b7d1ae99de35f1c7dddeb9549c50df");
        }

        private static void Case_00960()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 960,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,3,93,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-1,18,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-19,13,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,16,61,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,4,58,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,18,14,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-4,94,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,9,49,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,6,62,20,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "094b762855fa126b0010546929830a656b7608f42ad859e213da8c6790d7af86");
        }

        private static void Case_00961()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 961,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,9,45,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-19,79,36,1), new GeneratedEnemyUnit(8,-10,82,46,1), new GeneratedEnemyUnit(11,4,41,39,1), new GeneratedEnemyUnit(-13,4,78,16,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "86f5d98b9a3ca8fc92bfc50f6d9280f319fb12e239c98c471463713432b0a32b");
        }

        private static void Case_00962()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 962,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,5,62,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,12,52,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-11,98,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-9,80,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-16,84,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,13,66,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,12,37,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,1,43,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-20,24,19,2), new GeneratedEnemyUnit(19,-14,48,34,2), new GeneratedEnemyUnit(19,4,47,21,2), new GeneratedEnemyUnit(11,-14,90,48,4), new GeneratedEnemyUnit(-4,5,59,33,1), new GeneratedEnemyUnit(19,9,22,23,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "adb48dc8e39cb0bfd3e449b3d2547538f789d6ac04b90e36698fefb50770f867");
        }

        private static void Case_00963()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 963,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-13,53,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-6,42,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,2,58,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,16,63,37,1), new GeneratedEnemyUnit(-1,13,48,28,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "e9c367bb938378dbb7e766208117374ee8c50bf3800f0207b865a0b2e09b0310");
        }

        private static void Case_00964()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 964,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-7,92,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,4,13,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,19,96,37,1), new GeneratedEnemyUnit(16,-17,28,25,3), new GeneratedEnemyUnit(19,-2,95,16,3), new GeneratedEnemyUnit(-2,1,87,37,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "2f76f8c1b64b43b518939af3f4e690014fd796aae8090cda3fb01cf05dabf288");
        }

        private static void Case_00965()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 965,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-15,51,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-12,63,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,10,73,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,13,63,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-14,59,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-20,69,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-8,97,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,12,53,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,3,19,20,3), new GeneratedEnemyUnit(10,-4,32,37,2), new GeneratedEnemyUnit(-8,20,18,30,2), new GeneratedEnemyUnit(-17,4,84,6,3), new GeneratedEnemyUnit(-3,-13,7,25,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "01593488b57e63a64834327b5215932977cf37f6a62801a4a47bbc581331749f");
        }

        private static void Case_00966()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 966,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-15,78,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,2,89,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,16,34,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-12,13,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,18,83,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-13,92,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-4,57,27,2), new GeneratedEnemyUnit(-9,-11,99,26,3), new GeneratedEnemyUnit(11,2,20,7,4), new GeneratedEnemyUnit(-12,-8,28,8,2), new GeneratedEnemyUnit(1,12,38,7,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "8a6165181a28ca9682ed03d9878e9a4c52ef7db89bb6bd30b11ef5e78d9b18bb");
        }

        private static void Case_00967()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 967,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,3,97,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-5,36,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,13,46,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,17,47,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-3,97,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-2,95,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-15,90,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,19,69,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-18,83,27,3), new GeneratedEnemyUnit(-12,10,44,7,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "47ed10ccfaa3f6ff39e444e5dbf5f4f4924eda197cf4ba14b8959bc1cbe4736b");
        }

        private static void Case_00968()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 968,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-12,59,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,4,60,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,20,9,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,9,42,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-1,90,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,4,42,7,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,9,16,28,1), new GeneratedEnemyUnit(3,14,81,2,4), new GeneratedEnemyUnit(10,-15,97,26,3), new GeneratedEnemyUnit(6,6,89,47,2), new GeneratedEnemyUnit(12,10,53,13,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "ed18cfff2fe2433c9abbecf579a0c8ba2ebd1ed376bae4ec635ca2a86fec8f9d");
        }

        private static void Case_00969()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 969,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-1,78,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,3,9,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,13,10,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,9,94,5,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-16,28,6,4), new GeneratedEnemyUnit(-6,-5,45,43,3), new GeneratedEnemyUnit(-20,-11,15,9,3), new GeneratedEnemyUnit(6,17,50,9,1), new GeneratedEnemyUnit(7,14,51,49,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "8c2682e88a4535846911733adc144e8959dde974adc62509c24079c91fea7c49");
        }

        private static void Case_00970()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 970,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,1,89,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,3,25,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,18,69,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-15,98,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-8,31,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-19,55,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,1,30,30,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "fe79cacacab01eb18da5c57fb32dce2f98655b7a960857f49d9fda9c0386e291");
        }

        private static void Case_00971()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 971,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,9,36,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-16,87,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-4,38,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,8,78,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,3,62,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-10,17,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,15,21,40,3), new GeneratedEnemyUnit(6,4,37,39,2), new GeneratedEnemyUnit(-12,6,38,30,4), new GeneratedEnemyUnit(0,18,19,26,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "90379f88c79d6df9624ac429115e336b4d28d9459b8c2a03e1189898e59358f6");
        }

        private static void Case_00972()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 972,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,2,12,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-5,54,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,8,64,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-1,25,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-8,54,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,12,15,29,1), new GeneratedEnemyUnit(-14,-13,50,19,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "cedf3932cd1a206f8b2b46ba138c5dab124f875a26d3ee9fb10792d97f603db6");
        }

        private static void Case_00973()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 973,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,2,22,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,20,81,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,7,93,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,18,44,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-1,83,43,3), new GeneratedEnemyUnit(-19,-14,68,20,3), new GeneratedEnemyUnit(-10,-5,13,30,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "78d21e67f5bf324bed40c72c885542aa041ca2fb33aae21271e71f0a098298b8");
        }

        private static void Case_00974()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 974,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-6,52,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,3,63,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-3,70,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,1,16,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,14,6,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-13,75,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,12,66,39,1), new GeneratedEnemyUnit(3,-19,64,15,1), new GeneratedEnemyUnit(-1,-9,12,48,1), new GeneratedEnemyUnit(17,-18,77,45,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "37615b8ba79375564198539e5934cbb686165fb1adf4449d307f2f0f458a1962");
        }

        private static void Case_00975()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 975,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,14,80,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,6,13,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-1,64,34,2), new GeneratedEnemyUnit(20,15,66,16,3), new GeneratedEnemyUnit(-12,13,89,4,4), new GeneratedEnemyUnit(7,-19,98,35,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "f634567515c4aeefa7a62f95743811c556d1d0076c00f48063f441474307c2bd");
        }

        private static void Case_00976()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 976,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-5,82,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-17,50,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-2,17,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,16,13,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-10,76,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-16,45,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,14,52,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,9,29,37,3), new GeneratedEnemyUnit(-18,-11,37,25,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "486793dac67f540b1fd9fa760f1a01399e32e5d19f32992b77a12da9e4b0c250");
        }

        private static void Case_00977()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 977,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-4,59,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,17,44,5,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "21ff92452f78e2a6e98fdc7f536dce15786152485f2a9507771496a97230ee49");
        }

        private static void Case_00978()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 978,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,3,25,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-3,33,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-13,30,29,4), new GeneratedEnemyUnit(20,3,28,36,3), new GeneratedEnemyUnit(-16,-2,5,2,2), new GeneratedEnemyUnit(16,-11,66,23,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "3e7f80d1ec1ecab15c1455c0cd90b3366751bade8c8e78eafcd10ebf62c8e9d8");
        }

        private static void Case_00979()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 979,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,3,94,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,6,92,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,7,31,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-15,73,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,5,31,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,0,46,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-4,19,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,13,78,8,4), new GeneratedEnemyUnit(-8,-20,70,1,1), new GeneratedEnemyUnit(-16,-3,66,3,2), new GeneratedEnemyUnit(1,-10,92,23,3), new GeneratedEnemyUnit(19,-19,69,22,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "bde0cc063905188d6a054fede668f2a1d8569b4db6b83eec925c8ad9990c06c5");
        }

        private static void Case_00980()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 980,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-19,80,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,10,45,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,2,95,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-5,24,20,3), new GeneratedEnemyUnit(3,16,34,36,3), new GeneratedEnemyUnit(14,-16,69,34,1), new GeneratedEnemyUnit(0,18,84,34,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "3b5596ed2dd653d9cd343323effdad56e3428b4044e8694fcfba79c232a980ec");
        }

        private static void Case_00981()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 981,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-12,89,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,5,50,5,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "3aeb895118926e0c3b725f5b0bd842d4dbef1de5586ec63874459edac6b69d58");
        }

        private static void Case_00982()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 982,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-10,81,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-7,14,21,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "ba0be13a0029eba9ddd3841d16a64d14d0011766889482020d686ad5474b8612");
        }

        private static void Case_00983()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 983,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,13,81,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,0,6,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-5,23,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,19,97,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-4,27,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,10,81,42,2), new GeneratedEnemyUnit(-8,1,91,40,3), new GeneratedEnemyUnit(18,-5,14,17,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "5af0a2b59630d9690fe98a491b7b0eb4bea1069e00f6ee767668525002beaf6c");
        }

        private static void Case_00984()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 984,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,4,49,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,15,13,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-7,90,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-2,98,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,4,47,13,1), new GeneratedEnemyUnit(8,12,81,48,4), new GeneratedEnemyUnit(-2,-16,43,35,2), new GeneratedEnemyUnit(19,13,19,17,4), new GeneratedEnemyUnit(6,7,11,49,3), new GeneratedEnemyUnit(13,-10,59,50,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 55,
                stableHash: "ae9b02adbeead0b7e8082c6adaa01136896569478a0fd0df2e03c6f458e20eb7");
        }

        private static void Case_00985()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 985,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,20,52,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,1,66,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-4,78,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-10,90,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-17,18,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-14,72,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,8,91,5,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "7c86f0f5148ac189c1e28bf7d22984b5fa7d3eaa0f9322ffe5d904e5211e9daf");
        }

        private static void Case_00986()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 986,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,1,34,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-19,15,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,11,30,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-8,97,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,10,99,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,1,98,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,10,54,39,3), new GeneratedEnemyUnit(5,1,62,44,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 80,
                stableHash: "292a8755f2a512fd37f0515d21f9d5b2b61a151933866a15385c93dc1d8e5cb7");
        }

        private static void Case_00987()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 987,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,8,30,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-17,55,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,12,90,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,9,47,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,7,28,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,12,50,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,20,33,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-16,31,5,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-7,63,35,2), new GeneratedEnemyUnit(10,0,70,28,3), new GeneratedEnemyUnit(-14,20,46,7,1), new GeneratedEnemyUnit(-14,-4,74,12,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "8404be1230fa42deddbc6cbd632c04cd6a133eb09851a5261e14973fcbe201c3");
        }

        private static void Case_00988()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 988,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,10,95,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,6,48,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-4,51,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,8,37,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,2,60,1,3), new GeneratedEnemyUnit(14,8,89,43,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "b3f956e79a0781eae5fb68234abeeb10734a29aa64fbd2772f723e96c566fcef");
        }

        private static void Case_00989()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 989,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-13,56,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,15,43,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-15,24,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-20,9,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,0,8,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-14,50,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-1,28,41,3), new GeneratedEnemyUnit(8,-10,16,34,2), new GeneratedEnemyUnit(15,-10,88,12,3), new GeneratedEnemyUnit(18,-12,82,19,3), new GeneratedEnemyUnit(14,11,31,24,3), new GeneratedEnemyUnit(1,-10,18,34,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "5151ca65781f320292b3b0ad28b555f2bbd165afb43ef80bb33c0ee76b938fc6");
        }

        private static void Case_00990()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 990,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,3,92,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,4,78,4,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "72edd7288dc44f5a2275846ffb1b6b1a52b606dd58e39220692665c37e2dc229");
        }

        private static void Case_00991()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 991,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-19,59,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,20,25,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,13,37,12,3), new GeneratedEnemyUnit(-13,8,34,17,1), new GeneratedEnemyUnit(-7,-5,44,11,4), new GeneratedEnemyUnit(-7,-12,28,4,3), new GeneratedEnemyUnit(-20,19,41,31,3), new GeneratedEnemyUnit(13,-19,21,44,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "04734cc5e3edfb208a1508069ba278d259ea06b95f0ff0140ddb1417141619ab");
        }

        private static void Case_00992()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 992,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-16,68,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-5,28,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,5,18,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,12,73,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-12,70,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-5,12,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,4,82,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-3,63,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,3,40,38,2), new GeneratedEnemyUnit(18,1,5,30,2), new GeneratedEnemyUnit(-17,12,65,33,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "6cdb71ecf890abbc977d57a50b205ff22d948abae4e3d01ffc7b1f6f17b828ab");
        }

        private static void Case_00993()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 993,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,5,50,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-15,81,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-3,54,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,15,16,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,5,17,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-6,82,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-18,71,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,4,26,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,12,22,38,4), new GeneratedEnemyUnit(8,-14,41,11,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "44442c623ec4f6a986f996c3ae1c01f2320835924b3e088a0f47e3de04d1dfb9");
        }

        private static void Case_00994()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 994,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-19,36,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-13,21,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,20,70,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-18,81,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-7,64,48,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "aa5472e1b8072b6303dd872364dfd55525277cddb3515dec0b84c1c0c59bd6e9");
        }

        private static void Case_00995()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 995,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,2,5,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-3,45,5,3), new GeneratedEnemyUnit(-2,5,54,23,2), new GeneratedEnemyUnit(18,-11,24,50,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "bf0395653c82e9f6df7bcc39ffcd39e467af5aea689c3a88b96018e8596b1993");
        }

        private static void Case_00996()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 996,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-9,70,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-13,35,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,10,88,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,0,20,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-8,48,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,0,100,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-9,89,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-9,37,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-5,82,41,3), new GeneratedEnemyUnit(13,-1,40,36,2), new GeneratedEnemyUnit(-20,-3,32,36,4), new GeneratedEnemyUnit(9,-15,91,33,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "40b219b93a469bb08713284c6e28baa481fbe865e9eab93b5588f000eceec00a");
        }

        private static void Case_00997()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 997,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,0,22,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-14,98,31,2), new GeneratedEnemyUnit(12,15,50,10,1), new GeneratedEnemyUnit(-15,0,67,19,3), new GeneratedEnemyUnit(-13,-8,13,21,1), new GeneratedEnemyUnit(12,-4,88,29,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "2b88ea41ed79e9dafeb01548f5007637a41219340314293cec53ecb9b27680ba");
        }

        private static void Case_00998()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 998,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-17,86,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-20,31,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-5,80,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-5,90,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-4,74,27,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "4d76034c4d17278459a969d86dea1858b5de6a80881f0ace1153569cd3728cd5");
        }

        private static void Case_00999()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 999,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-5,75,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-19,94,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-11,36,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,12,13,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,15,80,5,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-4,84,11,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "e4990cb29f2e123a7ed15141bffb6411958cefda895fa885ea7b4682deacbb4c");
        }

        private static void Case_01000()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1000,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,20,96,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-11,35,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,13,41,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-6,29,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,5,92,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,3,47,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,2,79,16,4), new GeneratedEnemyUnit(14,18,49,29,4), new GeneratedEnemyUnit(17,-5,11,40,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "0007c978d2d99c958909adab2d967f747ad7ca77271f74173d319553981756d7");
        }

        private static void Case_01001()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1001,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,10,81,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-5,61,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,8,27,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,9,68,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-19,69,6,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 15,
                stableHash: "aa20d6b3692e7b5e0821273bd4a6e7ed9a14db8033163c806dfaf1a424f92579");
        }

        private static void Case_01002()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1002,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-15,96,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-10,16,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-5,18,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,11,32,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,4,84,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,18,35,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-16,56,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,8,66,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,3,70,24,4), new GeneratedEnemyUnit(19,-12,16,26,3), new GeneratedEnemyUnit(9,7,55,21,3), new GeneratedEnemyUnit(7,-20,63,28,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "29f68d192c633f35f4fd84d371d2ece21e52ce63343b620d558b8f7899a57d9f");
        }

        private static void Case_01003()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1003,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,2,32,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-4,52,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-8,63,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,1,20,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-4,9,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-7,43,9,4), new GeneratedEnemyUnit(12,-11,41,48,4), new GeneratedEnemyUnit(-1,-6,89,33,1), new GeneratedEnemyUnit(1,-15,61,10,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "f7b224d79168a14e00f2d6bdd65c799dce2cf845bb6be7bf6e443b9fad34246b");
        }

        private static void Case_01004()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1004,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-14,95,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,0,54,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-11,58,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-19,30,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-1,21,1,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "4586bf94bf5cd91c71a7884d91c87cae9b35b56941d2b45fb8bcc20db4b86594");
        }

        private static void Case_01005()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1005,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,20,35,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,16,31,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,13,96,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-14,31,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,13,88,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-13,58,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-2,94,25,3), new GeneratedEnemyUnit(5,-5,94,22,2), new GeneratedEnemyUnit(14,-5,40,34,3), new GeneratedEnemyUnit(-1,-2,79,27,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "056c8ce076295cf8eb66e5752ad9557e8390e88fb9c55e80c46465859c159856");
        }

        private static void Case_01006()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1006,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,4,65,4,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "4f966ec97bb758ff5deb10673ec999328cb45c984e015cd8927236104fa6e59a");
        }

        private static void Case_01007()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1007,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-1,49,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-9,100,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,12,40,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,7,58,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,16,82,15,1), new GeneratedEnemyUnit(-11,9,72,1,1), new GeneratedEnemyUnit(14,10,72,39,4), new GeneratedEnemyUnit(12,6,39,29,1), new GeneratedEnemyUnit(4,4,79,50,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "bb44ab654dd5c701648873d3032c57300f2ddb910bf3aba23634689adef50ed5");
        }

        private static void Case_01008()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1008,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-11,46,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-3,89,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-12,65,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,19,65,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,15,31,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,7,94,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-5,57,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,1,58,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,3,5,5,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "8d1296b93b92073f25a406c18e329ba55ae7a724b5e990565677adaf07f96cd0");
        }

        private static void Case_01009()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1009,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-5,67,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,4,90,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-6,80,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-6,26,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-10,84,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,18,8,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-13,18,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-1,71,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-18,35,9,4), new GeneratedEnemyUnit(1,-17,17,16,4), new GeneratedEnemyUnit(-3,-13,14,32,2), new GeneratedEnemyUnit(18,-14,24,6,3), new GeneratedEnemyUnit(-4,-16,68,25,3), new GeneratedEnemyUnit(19,5,93,48,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "56cb8daf3ff5b2af98b7931e04f07bab92cac9507eecc6395ba15e6632f8595e");
        }

        private static void Case_01010()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1010,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-16,92,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-8,77,21,4), new GeneratedEnemyUnit(-10,14,6,36,3), new GeneratedEnemyUnit(3,13,84,36,4), new GeneratedEnemyUnit(-16,13,23,46,2), new GeneratedEnemyUnit(7,-17,40,14,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "598d02006098cb016daaff516f2b448e8df971723cbca000e400b2e6bbe0b987");
        }

        private static void Case_01011()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1011,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,14,41,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-19,87,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,1,45,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-20,18,6,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "ab05abf74fcfe1a214752637a4b43f7aadd1cdf0925f074adde25e25d4566759");
        }

        private static void Case_01012()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1012,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-7,98,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,1,58,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,2,93,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,18,6,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,2,50,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-6,7,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,14,42,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,7,81,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,6,93,47,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "e101eae9b08511f43f1827e58aa86605dfe7358a82dd9157e8b8b4d3e10af491");
        }

        private static void Case_01013()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1013,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,19,38,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-9,61,4,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "0bd8f1e8e095a3619376ef8a242827ebd7ae111ead103f3da8de95b04ce7c321");
        }

        private static void Case_01014()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1014,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-17,12,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,12,16,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-10,79,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,15,23,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-9,95,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,9,90,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-20,79,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,8,49,4,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "aa8c5e649f0861973e6fb117266abf6e1465fd5664c8157796cd1beefa21e84c");
        }

        private static void Case_01015()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1015,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-18,6,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,17,52,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,12,29,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,19,49,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,3,43,28,2), new GeneratedEnemyUnit(7,2,70,40,4), new GeneratedEnemyUnit(19,-15,38,16,3), new GeneratedEnemyUnit(-13,18,81,31,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "ca7052003b9322035b5f0f4fb70d2ee57c1079285392d116ca03389432caeffa");
        }

        private static void Case_01016()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1016,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-8,38,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,18,65,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,2,61,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-6,49,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-17,8,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-6,47,47,4), new GeneratedEnemyUnit(3,14,35,21,1), new GeneratedEnemyUnit(0,-10,48,1,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "377e230d0d1693230377b25b3d6917f930c35cf67d9ef3c4f7d375d0edbaabc7");
        }

        private static void Case_01017()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1017,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,3,88,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,2,72,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,6,90,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,15,37,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-3,34,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,3,54,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-9,49,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-15,56,2,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,11,79,41,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "0ba276f5adb4d47e08c9af90f1dd9343783c48479741ce5325ff4504aae5a5fe");
        }

        private static void Case_01018()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1018,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-20,5,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-20,61,16,3), new GeneratedEnemyUnit(0,-14,36,31,3), new GeneratedEnemyUnit(8,4,44,20,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "4fc02ccbdc01128691bc9ceb3b7ba8b36ac3f92bdd4fb841deee721265a48596");
        }

        private static void Case_01019()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1019,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-17,21,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-20,69,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,2,91,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,3,24,14,1), new GeneratedEnemyUnit(-11,-3,26,7,3), new GeneratedEnemyUnit(9,9,100,13,1), new GeneratedEnemyUnit(-8,-13,22,39,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "b836f0c589aff25eedd62e62cf5fc702b8c72f3ae368a43ec6e659f579a5ee12");
        }

        private static void Case_01020()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1020,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,20,86,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-14,53,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,17,47,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-2,50,17,4), new GeneratedEnemyUnit(-8,-10,67,37,2), new GeneratedEnemyUnit(6,20,24,46,2), new GeneratedEnemyUnit(0,-5,33,23,3), new GeneratedEnemyUnit(9,13,49,34,1), new GeneratedEnemyUnit(20,-11,35,30,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "9c659c2b205fa60c6b6e4aa711a8fc4424d27c0c2a089476b7da5f69569b08c3");
        }

        private static void Case_01021()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1021,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,17,39,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,14,60,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,0,59,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-1,21,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-14,93,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,16,64,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-7,11,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,8,77,18,2), new GeneratedEnemyUnit(-19,2,21,21,3), new GeneratedEnemyUnit(-3,11,24,25,1), new GeneratedEnemyUnit(-7,5,64,12,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "f0b48b566fde19a35a7104e80bfacee40f390ae43f1c49ae5b83b617b2de4764");
        }

        private static void Case_01022()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1022,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,8,57,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,19,12,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,13,84,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-10,18,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,14,65,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-16,57,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,10,43,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-17,25,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,20,30,50,2), new GeneratedEnemyUnit(17,4,20,45,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "4929b746caad758c5fa027d6fb703e109e7e386fe53a68c71d7f105d187155fa");
        }

        private static void Case_01023()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1023,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,14,86,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,19,59,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-4,43,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-4,32,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,5,45,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-19,100,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-2,9,4,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,4,10,21,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "5b82717d45d69831565ab795e59c905b8a843237a4641b582213a2888ef14764");
        }

        private static void Case_01024()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1024,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,10,16,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,1,21,6,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 15,
                stableHash: "aade16d1739ee7e9c73f5712277dbfb9ed9a9af8cbe0761ae9c5a890fcfedfb6");
        }

        private static void Case_01025()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1025,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-4,65,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,10,65,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,19,58,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,12,60,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-11,100,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,10,51,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,18,48,20,4), new GeneratedEnemyUnit(1,13,15,18,2), new GeneratedEnemyUnit(7,2,45,24,4), new GeneratedEnemyUnit(11,6,52,49,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "246115a39e14a33c2562374089045940c7dbdfd3e10a95ffecf2e0f9a12f83c3");
        }

        private static void Case_01026()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1026,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-7,74,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-7,74,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-4,80,23,1), new GeneratedEnemyUnit(4,2,76,33,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "52effeefc10e19047f3dde44977c2a35f6c0fa6a083c239dbdf440ccb91a8f0f");
        }

        private static void Case_01027()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1027,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,5,38,5,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "f749bd9e931694ed947e2b74040a586ae504582d122a255522ce04248a7b967f");
        }

        private static void Case_01028()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1028,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-12,51,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,5,31,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,18,53,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-14,87,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-4,72,46,3), new GeneratedEnemyUnit(-15,6,96,25,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "d1bd4c1d076750fefffe05b47a0c40388de7d752fd5a98ff53fdf7d85b3b1a8d");
        }

        private static void Case_01029()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1029,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-10,15,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,9,30,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-3,87,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,18,50,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-12,18,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,4,79,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,11,46,41,2), new GeneratedEnemyUnit(-10,-12,65,49,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "73ed8d68532943c582a0ffe6dc9ba179429901b6b30addf11f4e9906dcfad20f");
        }

        private static void Case_01030()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1030,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-11,70,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-12,19,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-14,93,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,3,33,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,20,27,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,6,69,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-9,89,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-7,39,44,3), new GeneratedEnemyUnit(10,18,15,25,4), new GeneratedEnemyUnit(-12,-17,69,17,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "80199871847ff7cd1359465e2cdaddfcf569a804ce652865ec89f64aede09137");
        }

        private static void Case_01031()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1031,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,8,7,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,4,93,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-2,46,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-11,80,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,5,44,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,7,74,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-10,9,28,2), new GeneratedEnemyUnit(-4,-17,27,41,1), new GeneratedEnemyUnit(-16,-16,58,49,2), new GeneratedEnemyUnit(0,19,94,42,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "36815a77dcbbe99a94181290ce45e58faf048c1747a780a161082989175ddab9");
        }

        private static void Case_01032()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1032,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-15,28,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-20,16,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-17,71,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-4,48,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,7,57,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-11,5,38,3), new GeneratedEnemyUnit(17,-1,50,33,3), new GeneratedEnemyUnit(-4,-19,82,13,1), new GeneratedEnemyUnit(-11,13,88,6,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "b35feaffc04c699315ecbb884e57d1d60d2b1e437492c015430023ef91f25d85");
        }

        private static void Case_01033()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1033,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-18,18,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-3,46,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,12,47,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,19,15,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,9,35,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-6,94,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,15,97,21,4), new GeneratedEnemyUnit(5,-5,95,7,1), new GeneratedEnemyUnit(-20,14,37,3,2), new GeneratedEnemyUnit(18,15,95,38,1), new GeneratedEnemyUnit(2,0,93,10,4), new GeneratedEnemyUnit(-7,-7,45,46,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "3175fa4f20d34819e732fcb9879eeac531f6ae9670f7bc3fdb8204080309489d");
        }

        private static void Case_01034()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1034,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-20,94,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,18,52,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,6,73,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-8,51,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,19,78,49,4), new GeneratedEnemyUnit(13,-3,48,4,2), new GeneratedEnemyUnit(5,-1,69,20,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "70326978fde3f53a973427061df6768edb22c334d493d5492a9d7dec395c6d99");
        }

        private static void Case_01035()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1035,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-6,75,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,5,66,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,15,54,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,18,48,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,20,58,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-20,24,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,16,56,36,4), new GeneratedEnemyUnit(7,5,66,3,2), new GeneratedEnemyUnit(13,1,86,26,4), new GeneratedEnemyUnit(-11,-18,74,12,3), new GeneratedEnemyUnit(-18,-7,87,2,2), new GeneratedEnemyUnit(7,-16,66,27,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "65d41a6851e8b00d7df8dfae1d30a3d6c315343b6f484285e508b07de2c12c82");
        }

        private static void Case_01036()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1036,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,9,34,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-6,31,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-20,6,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-2,36,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,2,74,27,2), new GeneratedEnemyUnit(17,-15,17,23,4), new GeneratedEnemyUnit(-15,9,26,46,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "41b397445c444f180f7bb42c63053caa3c4358d8ef1eb38c5bf4a3ee7ad076e9");
        }

        private static void Case_01037()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1037,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,18,24,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,7,74,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-12,95,4,3), new GeneratedEnemyUnit(-10,7,24,44,1), new GeneratedEnemyUnit(-2,13,99,39,1), new GeneratedEnemyUnit(-17,4,94,35,3), new GeneratedEnemyUnit(3,-19,86,42,1), new GeneratedEnemyUnit(5,-13,60,44,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "413aa8d415869a04fcd6995c4b3f9aa609df535e634736584e1cae1c42a29a61");
        }

        private static void Case_01038()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1038,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,9,88,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-19,46,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-3,37,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-4,95,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-9,49,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-17,38,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,17,43,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-3,91,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-2,26,22,3), new GeneratedEnemyUnit(4,6,61,31,1), new GeneratedEnemyUnit(10,-19,17,16,3), new GeneratedEnemyUnit(11,14,46,34,4), new GeneratedEnemyUnit(-16,10,20,38,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 55,
                stableHash: "aca0bf667df46729a692c23a77665cdcf8965b37b234bf41b23852a9462ae9dd");
        }

        private static void Case_01039()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1039,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-8,80,1,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "9dfa05dc46f974706676989b6050b8fc1f497c78fb1cf0b54df44b2e9e1566b1");
        }

        private static void Case_01040()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1040,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,8,42,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-8,6,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,10,46,30,1), new GeneratedEnemyUnit(17,3,26,8,2), new GeneratedEnemyUnit(0,-10,84,27,3), new GeneratedEnemyUnit(-4,-14,7,16,2), new GeneratedEnemyUnit(15,-4,70,2,1), new GeneratedEnemyUnit(4,-18,60,10,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "93bc9c7815a9bfb25895a86980089851e8164237bfc6878bacdc4347e9699504");
        }

        private static void Case_01041()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1041,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-5,88,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-2,75,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-3,10,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,4,27,27,4), new GeneratedEnemyUnit(-15,-18,39,34,2), new GeneratedEnemyUnit(-13,4,66,48,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "3b48be098fe9c3d4e8c121e0b0030e165a959be7f08c96b6fa1bba447a142913");
        }

        private static void Case_01042()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1042,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,10,31,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,7,51,3,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "d0206371df727a66fd7f428e783329182435327eccc0ca3aee53886e4982fea3");
        }

        private static void Case_01043()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1043,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,18,11,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,11,40,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-5,30,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-13,65,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,16,9,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,2,74,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,4,48,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-4,77,1,2), new GeneratedEnemyUnit(12,1,34,36,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "7b0cc80c3ab3989db7d66d492b54e4b0f99a4087370e9229877a368e6a6e19f3");
        }

        private static void Case_01044()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1044,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-20,46,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,13,52,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-11,27,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-7,62,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-13,14,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,11,16,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-13,57,21,3), new GeneratedEnemyUnit(-14,10,51,4,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "1bc4a595bfca7db2204afe739fe38aa05b1d452adfbb4aa57b2f609cf7de5391");
        }

        private static void Case_01045()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1045,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,15,86,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,11,61,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,20,32,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,4,30,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,19,72,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,13,12,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-12,42,35,4), new GeneratedEnemyUnit(-9,-12,5,23,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "c563b0b4bda88f0a9e88146df8003b994dd68c6364e4aff55a5cf832c90f2750");
        }

        private static void Case_01046()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1046,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,16,78,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-3,58,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,5,77,29,2), new GeneratedEnemyUnit(-13,-19,87,3,2), new GeneratedEnemyUnit(11,10,43,1,3), new GeneratedEnemyUnit(-14,-16,51,23,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "7c01a3bf993f7c7a6e6674e9243fec99929f20babd0044aaf1d007e40fa90644");
        }

        private static void Case_01047()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1047,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,18,44,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,12,15,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-8,33,5,1), new GeneratedEnemyUnit(-7,7,100,34,3), new GeneratedEnemyUnit(3,18,64,38,2), new GeneratedEnemyUnit(-4,14,66,25,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "01f4866f8e170d63350ea88148b5e65162772e4f968c6948b17220969bcd8cfc");
        }

        private static void Case_01048()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1048,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-17,81,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,20,86,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,7,87,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-18,84,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-3,69,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-11,94,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,4,8,11,2), new GeneratedEnemyUnit(-3,17,92,21,4), new GeneratedEnemyUnit(14,-14,55,24,2), new GeneratedEnemyUnit(-17,19,94,21,3), new GeneratedEnemyUnit(-5,11,38,37,4), new GeneratedEnemyUnit(-6,-18,61,20,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "5b5e65ec4f5b7ddae6e59f5fc0cabfbb7ee9e6c79323fc4bd381e23456e18915");
        }

        private static void Case_01049()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1049,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,2,85,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,4,57,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-3,6,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-19,22,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,5,20,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-5,39,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-18,91,39,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "58b731bc9e426db1b00d1e3d2d3652776e15f687fd627b1a6a007a34d554afa9");
        }

        private static void Case_01050()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1050,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-19,13,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,0,96,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,0,87,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,9,36,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-5,86,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-14,28,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,12,18,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,7,91,50,1), new GeneratedEnemyUnit(10,17,59,13,3), new GeneratedEnemyUnit(-10,7,16,25,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "a91cd70ecd37febe2cf63b9c57b3402a943c053e09c39d39c517176ec2eb291c");
        }

        private static void Case_01051()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1051,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,20,95,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,1,40,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,2,21,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-13,12,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,2,6,46,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "67faffffd0975cca18781288a377fab79c8307b235ce9a2be67962101a1d5dd7");
        }

        private static void Case_01052()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1052,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,16,93,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,18,93,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-6,26,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,5,27,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,11,66,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,2,44,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,17,83,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-15,80,43,1), new GeneratedEnemyUnit(-20,3,19,19,4), new GeneratedEnemyUnit(-7,-12,39,40,1), new GeneratedEnemyUnit(-5,-7,68,33,3), new GeneratedEnemyUnit(-19,-9,18,40,3), new GeneratedEnemyUnit(16,3,48,47,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 40,
                stableHash: "db71ab24441aab62484030c8f36b580839a75f7577c7c1efc68ba8014c2abedc");
        }

        private static void Case_01053()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1053,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-19,37,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-17,13,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,3,13,31,2), new GeneratedEnemyUnit(6,5,54,1,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "ca28e3d90db266b306c1a258b248092dfccae2cef639005d0d3b68ff019f5cb9");
        }

        private static void Case_01054()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1054,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,9,32,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-12,35,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,7,13,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-13,22,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,10,26,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,7,91,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-1,86,31,3), new GeneratedEnemyUnit(1,4,11,1,1), new GeneratedEnemyUnit(17,16,15,3,2), new GeneratedEnemyUnit(2,-9,42,44,2), new GeneratedEnemyUnit(16,2,84,21,4), new GeneratedEnemyUnit(3,9,43,22,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "66cf0a23046d53d90dbed60b4baa80f5e3a5413fb436a3b402e1fd495590da0a");
        }

        private static void Case_01055()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1055,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,7,5,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,17,88,39,4), new GeneratedEnemyUnit(6,17,78,38,4), new GeneratedEnemyUnit(-15,7,66,9,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "eecad4824c10734df58827f06ca6a65b04a4b956974b675c04719b1773ad5155");
        }

        private static void Case_01056()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1056,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,1,66,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-12,66,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-6,16,40,4), new GeneratedEnemyUnit(-11,12,53,40,3), new GeneratedEnemyUnit(2,-12,89,24,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "3cab034a8b947c13c6a82d443b37182217ce8bb9dbefd84325c9619b0170c457");
        }

        private static void Case_01057()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1057,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-5,100,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-17,35,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,8,10,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-4,83,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-4,41,40,1), new GeneratedEnemyUnit(19,-16,62,14,4), new GeneratedEnemyUnit(8,-7,95,32,1), new GeneratedEnemyUnit(-19,1,23,7,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "8c195cf545e34bbe0bf35368b198b5a4271bd4631d993b85eb9a619bd1f09509");
        }

        private static void Case_01058()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1058,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,4,68,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-17,49,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,10,11,11,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 20,
                stableHash: "f6b31bbd514252b5ac81be55f22b07eebb46147b931d715461d0255017fe60e8");
        }

        private static void Case_01059()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1059,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,8,92,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,17,46,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-14,76,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,9,89,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,1,21,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,7,93,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-4,88,35,2), new GeneratedEnemyUnit(10,14,34,27,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "fe898593b460c2fb39ae2a847de99268093bba85a70ec934ef5b02be55ae727d");
        }

        private static void Case_01060()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1060,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,17,55,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-12,49,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,16,46,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-14,82,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-14,69,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,16,16,30,3), new GeneratedEnemyUnit(-14,-8,67,42,3), new GeneratedEnemyUnit(-7,9,53,28,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "c8543475a3613bf50e53a026016edc14c3553c37437bc08d61506c75cce0e809");
        }

        private static void Case_01061()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1061,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-6,22,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,1,56,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-5,100,43,3), new GeneratedEnemyUnit(-19,5,95,33,1), new GeneratedEnemyUnit(-8,19,36,18,4), new GeneratedEnemyUnit(12,-5,74,19,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "7991eefb4e51a6c72aa62023d7b29c9240acb0dc0030072240fefdae934b803c");
        }

        private static void Case_01062()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1062,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-2,17,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,2,36,48,1), new GeneratedEnemyUnit(-15,9,30,15,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "109cc766cf050e517eb7f6efbee9b1b9c0b33347e93c2d5223f5fb62f9acb980");
        }

        private static void Case_01063()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1063,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,6,70,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,17,63,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,18,54,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,8,89,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-15,18,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,15,16,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-15,88,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,7,79,48,4), new GeneratedEnemyUnit(12,17,32,28,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "04f3aa6b95c9e19ec94bed0dc97fb6499ca3d682f64846b8be14a3c17e34145c");
        }

        private static void Case_01064()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1064,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-13,26,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-10,60,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-9,90,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-15,57,25,4), new GeneratedEnemyUnit(9,6,54,50,2), new GeneratedEnemyUnit(7,14,74,27,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "8c95063d88d59b3b765f1bfa24de7f98d119cbf4b737738d39ab32001e70a91e");
        }

        private static void Case_01065()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1065,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-3,86,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-16,69,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,13,63,27,2), new GeneratedEnemyUnit(-17,-16,82,37,1), new GeneratedEnemyUnit(-5,-13,94,23,2), new GeneratedEnemyUnit(3,20,100,17,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "991d548b6aeab127d446aca3555cf0013e8dea504aa7a0ebc4e811a4c3c32c34");
        }

        private static void Case_01066()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1066,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-13,80,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,4,36,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,19,35,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,16,91,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,15,73,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-17,11,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-10,22,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,3,76,25,4), new GeneratedEnemyUnit(14,-19,30,9,1), new GeneratedEnemyUnit(2,7,11,35,4), new GeneratedEnemyUnit(-1,6,86,8,3), new GeneratedEnemyUnit(11,15,97,1,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "a4ee3e9a4c19c795472f3b7393bca36842f75b9f67447ae558d0d3cce8c601e2");
        }

        private static void Case_01067()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1067,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,12,98,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-16,36,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,17,34,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-12,81,35,2), new GeneratedEnemyUnit(1,11,30,41,1), new GeneratedEnemyUnit(16,1,74,49,3), new GeneratedEnemyUnit(20,-18,91,6,1), new GeneratedEnemyUnit(11,-20,23,14,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "b60f987c99a202db49c119036d3ddb124f3dade728c0e5e26137df20029727ad");
        }

        private static void Case_01068()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1068,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,16,77,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,14,10,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,16,14,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,15,22,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,0,67,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,7,64,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,1,91,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,15,11,8,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "905f977889f481b56988b502fda86171d2d590369091e78b0e4a58706836a16e");
        }

        private static void Case_01069()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1069,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-9,66,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-2,99,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,12,5,17,4), new GeneratedEnemyUnit(18,4,77,43,4), new GeneratedEnemyUnit(-6,12,48,9,4), new GeneratedEnemyUnit(-3,-1,56,3,1), new GeneratedEnemyUnit(5,14,20,44,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "da226925f851a735e1eb1f1b68f79bb9223a8cadb8f518e9cafcc8d12e4016d3");
        }

        private static void Case_01070()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1070,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-10,83,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-9,6,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-2,26,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,6,56,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-6,77,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-19,32,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,4,34,32,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "674bf0586119856ba33e41cd560fa4f065062179482f0ce118d83d59cce59a6c");
        }

        private static void Case_01071()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1071,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,13,71,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-10,84,47,2), new GeneratedEnemyUnit(7,20,25,36,1), new GeneratedEnemyUnit(13,-2,22,37,3), new GeneratedEnemyUnit(-19,-2,60,23,4), new GeneratedEnemyUnit(-1,-11,89,42,1), new GeneratedEnemyUnit(-14,-10,81,8,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "32b8ed7f4b3bcf193c4034d81cc6ca4c123194d3889e9df976e8bbfc203f530a");
        }

        private static void Case_01072()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1072,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-3,46,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-16,50,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-7,28,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-8,12,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-7,58,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,6,95,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,9,41,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,18,45,1,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "a3d7c990802c9694a9780da082d56d4f4995067ee222774545acc9f98d70d268");
        }

        private static void Case_01073()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1073,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,12,55,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,19,100,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,16,71,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,6,46,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,16,69,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,10,55,14,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "9b0e577d79a26929a6ab53ae8b588c9e8cdc5431d9d249b5cbb5c110b322703f");
        }

        private static void Case_01074()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1074,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,6,12,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,20,62,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-16,11,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-13,82,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-13,14,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,9,31,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-14,12,32,2), new GeneratedEnemyUnit(-13,3,73,22,2), new GeneratedEnemyUnit(-19,-18,60,6,2), new GeneratedEnemyUnit(-16,-14,76,5,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "7b4c345223ef8daefe5f1da2a71949e20fb9b45e708d2f26a6959bdb9098a6c6");
        }

        private static void Case_01075()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1075,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-17,50,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,2,58,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-5,84,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,16,89,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,2,11,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,13,54,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-6,45,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,15,30,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-8,50,38,2), new GeneratedEnemyUnit(11,11,40,35,4), new GeneratedEnemyUnit(11,-1,91,7,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "9ef9ebf38a8218b95746b7f5a8dbd5f88df8dc2e99a41bb042bc40ab5b5de393");
        }

        private static void Case_01076()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1076,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,8,29,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-20,22,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-5,48,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,16,60,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-12,86,26,4), new GeneratedEnemyUnit(-17,18,48,27,3), new GeneratedEnemyUnit(-8,2,23,34,1), new GeneratedEnemyUnit(-19,13,81,8,1), new GeneratedEnemyUnit(-9,5,78,47,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "de54e61fe2f263204eaddac07894430b315eadd58896c08c6b83b24899be5864");
        }

        private static void Case_01077()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1077,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-16,9,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,0,36,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,1,95,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-10,54,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,18,17,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,17,38,43,1), new GeneratedEnemyUnit(-2,-10,8,32,3), new GeneratedEnemyUnit(-4,10,17,42,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "02d233c7d2722b8ca2014065a48c1d32d743de0c84a25ed820f72f6e7f92a9c6");
        }

        private static void Case_01078()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1078,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-18,57,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,7,25,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-4,71,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-2,82,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,4,73,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-10,75,4,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "443fd549efad78c0f97508a746084f387fc8f4d8c0704a60b5408ed9f7e2082a");
        }

        private static void Case_01079()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1079,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,15,72,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-20,74,8,3), new GeneratedEnemyUnit(-1,5,71,16,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "13165e53e09318601bff49ad473e3c56122d05915aabe7d5d6a0433ace3e8cc4");
        }

    }
}
