using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard023
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_04140();
            Case_04141();
            Case_04142();
            Case_04143();
            Case_04144();
            Case_04145();
            Case_04146();
            Case_04147();
            Case_04148();
            Case_04149();
            Case_04150();
            Case_04151();
            Case_04152();
            Case_04153();
            Case_04154();
            Case_04155();
            Case_04156();
            Case_04157();
            Case_04158();
            Case_04159();
            Case_04160();
            Case_04161();
            Case_04162();
            Case_04163();
            Case_04164();
            Case_04165();
            Case_04166();
            Case_04167();
            Case_04168();
            Case_04169();
            Case_04170();
            Case_04171();
            Case_04172();
            Case_04173();
            Case_04174();
            Case_04175();
            Case_04176();
            Case_04177();
            Case_04178();
            Case_04179();
            Case_04180();
            Case_04181();
            Case_04182();
            Case_04183();
            Case_04184();
            Case_04185();
            Case_04186();
            Case_04187();
            Case_04188();
            Case_04189();
            Case_04190();
            Case_04191();
            Case_04192();
            Case_04193();
            Case_04194();
            Case_04195();
            Case_04196();
            Case_04197();
            Case_04198();
            Case_04199();
            Case_04200();
            Case_04201();
            Case_04202();
            Case_04203();
            Case_04204();
            Case_04205();
            Case_04206();
            Case_04207();
            Case_04208();
            Case_04209();
            Case_04210();
            Case_04211();
            Case_04212();
            Case_04213();
            Case_04214();
            Case_04215();
            Case_04216();
            Case_04217();
            Case_04218();
            Case_04219();
            Case_04220();
            Case_04221();
            Case_04222();
            Case_04223();
            Case_04224();
            Case_04225();
            Case_04226();
            Case_04227();
            Case_04228();
            Case_04229();
            Case_04230();
            Case_04231();
            Case_04232();
            Case_04233();
            Case_04234();
            Case_04235();
            Case_04236();
            Case_04237();
            Case_04238();
            Case_04239();
            Case_04240();
            Case_04241();
            Case_04242();
            Case_04243();
            Case_04244();
            Case_04245();
            Case_04246();
            Case_04247();
            Case_04248();
            Case_04249();
            Case_04250();
            Case_04251();
            Case_04252();
            Case_04253();
            Case_04254();
            Case_04255();
            Case_04256();
            Case_04257();
            Case_04258();
            Case_04259();
            Case_04260();
            Case_04261();
            Case_04262();
            Case_04263();
            Case_04264();
            Case_04265();
            Case_04266();
            Case_04267();
            Case_04268();
            Case_04269();
            Case_04270();
            Case_04271();
            Case_04272();
            Case_04273();
            Case_04274();
            Case_04275();
            Case_04276();
            Case_04277();
            Case_04278();
            Case_04279();
            Case_04280();
            Case_04281();
            Case_04282();
            Case_04283();
            Case_04284();
            Case_04285();
            Case_04286();
            Case_04287();
            Case_04288();
            Case_04289();
            Case_04290();
            Case_04291();
            Case_04292();
            Case_04293();
            Case_04294();
            Case_04295();
            Case_04296();
            Case_04297();
            Case_04298();
            Case_04299();
            Case_04300();
            Case_04301();
            Case_04302();
            Case_04303();
            Case_04304();
            Case_04305();
            Case_04306();
            Case_04307();
            Case_04308();
            Case_04309();
            Case_04310();
            Case_04311();
            Case_04312();
            Case_04313();
            Case_04314();
            Case_04315();
            Case_04316();
            Case_04317();
            Case_04318();
            Case_04319();
        }

        private static void Case_04140()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4140,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-2,51,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-9,90,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,17,7,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-2,99,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-3,68,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,11,95,12,1), new GeneratedEnemyUnit(15,5,37,14,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "fe0c5659fc0ed69f8edff827ee3c63a3a3224eb1b5c36935d42503ab8ec9bcff");
        }

        private static void Case_04141()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4141,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,0,22,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,11,56,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,12,56,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-17,92,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-11,90,27,2), new GeneratedEnemyUnit(15,-13,23,3,4), new GeneratedEnemyUnit(8,9,43,32,4), new GeneratedEnemyUnit(10,-3,41,12,2), new GeneratedEnemyUnit(20,8,61,42,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "b435793f3ddd67627be7c6fd586825e8084347bfb41a80ae24dd0ac93c7b2eca");
        }

        private static void Case_04142()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4142,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-20,36,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-12,100,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-16,99,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-16,46,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-5,74,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-7,55,2,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "891f2eea7c48e3e783fbd45e4fce5f8472ea1d0ee96548eab99fb17bd77a6edc");
        }

        private static void Case_04143()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4143,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,0,49,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,3,66,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,18,75,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,3,45,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-17,36,1,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "5a1216b7126e5de34f2a4f6b077d446dc516047151c073846885ac6fddb96c97");
        }

        private static void Case_04144()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4144,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-14,32,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,18,68,17,1), new GeneratedEnemyUnit(10,5,9,5,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "0fc2749d6c86baeefc7c7959b1cefa0ccf915413141e99bc88c3a82c1fb82fb0");
        }

        private static void Case_04145()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4145,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-12,24,4,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "3c6f154a395723c6694a78adf68a484e707804dec706df1d5aa6fc7b16450d16");
        }

        private static void Case_04146()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4146,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-3,16,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,14,16,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-1,68,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,10,21,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,0,63,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,19,17,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,10,79,10,2), new GeneratedEnemyUnit(11,-2,80,5,1), new GeneratedEnemyUnit(13,-8,66,23,1), new GeneratedEnemyUnit(3,10,82,23,4), new GeneratedEnemyUnit(-1,-16,39,33,2), new GeneratedEnemyUnit(-2,4,20,25,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 45,
                stableHash: "02cf81f48fb668c504089258bb5dc0fdbfb1a1f5d8a7cfdba6fefa85422b2982");
        }

        private static void Case_04147()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4147,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,18,75,5,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "53d72d3b28fda20d22f3d103a2f63f54fecd765c24598865b9b0cd48b00425e7");
        }

        private static void Case_04148()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4148,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-2,29,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,5,76,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-7,77,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-9,23,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,8,97,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,11,27,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,12,31,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-17,13,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,16,98,39,4), new GeneratedEnemyUnit(-16,9,65,15,4), new GeneratedEnemyUnit(4,11,36,7,2), new GeneratedEnemyUnit(3,-11,65,23,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "68cd65f255abbc059acd8bbb2d00b839aa420f3ddca4ce446e8d30857b1b653a");
        }

        private static void Case_04149()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4149,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,2,77,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-12,53,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-9,41,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-11,21,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-8,51,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,12,6,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,2,66,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-17,89,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-12,23,43,3), new GeneratedEnemyUnit(11,-12,64,21,2), new GeneratedEnemyUnit(4,14,90,5,4), new GeneratedEnemyUnit(20,5,26,9,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "229077401de5d6c4b27b0d88a10d198185be381485341ae45d886bae3ede3545");
        }

        private static void Case_04150()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4150,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,1,49,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,4,64,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,18,56,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,13,29,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-12,71,17,2), new GeneratedEnemyUnit(13,-1,27,16,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "a33a1d3ac9388d16239a5327ee851491df066b29eaffa29c7af9acc0508b750e");
        }

        private static void Case_04151()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4151,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,14,49,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-6,33,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-15,85,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,18,88,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-20,35,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,20,26,4,4), new GeneratedEnemyUnit(-3,2,45,34,3), new GeneratedEnemyUnit(-17,11,55,25,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "91f9de7746349057a55709f54c7b54836095569b01d3055bedf273c85a7b3502");
        }

        private static void Case_04152()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4152,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-3,8,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,1,60,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,13,18,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-9,79,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-9,23,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-17,95,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,6,25,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,8,5,6,3), new GeneratedEnemyUnit(-10,-18,35,40,3), new GeneratedEnemyUnit(-15,-7,44,39,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "db47ec68289c040277573481f952a39ce5ac985afe6ede21382b95fe01913647");
        }

        private static void Case_04153()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4153,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,18,82,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,1,52,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,5,43,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-9,74,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,16,53,2,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "53157ed96eac397dd6ab1830b63d3eddf21d7d3bbc9b4224127cc5fb52ae2cb2");
        }

        private static void Case_04154()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4154,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-6,39,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,2,26,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,15,21,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,12,39,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-8,91,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-12,44,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-3,33,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-4,87,49,1), new GeneratedEnemyUnit(-6,17,16,39,1), new GeneratedEnemyUnit(-1,2,60,9,1), new GeneratedEnemyUnit(-5,-18,38,26,4), new GeneratedEnemyUnit(2,-3,45,27,2), new GeneratedEnemyUnit(-1,6,27,15,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "ccda94cdb6c42ebe6c04141416afc8eae0423a3716273b6ad75276365105c63a");
        }

        private static void Case_04155()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4155,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-14,56,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-12,7,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,7,84,46,4), new GeneratedEnemyUnit(19,-7,98,34,1), new GeneratedEnemyUnit(-7,4,42,16,1), new GeneratedEnemyUnit(-18,12,88,12,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "04457d04adaabc1829606c7675ff09b1514213dcdb46ae17d7cab9a78d075f36");
        }

        private static void Case_04156()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4156,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-14,48,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-1,22,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-10,37,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,0,55,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,18,76,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,3,55,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,5,53,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-8,25,50,1), new GeneratedEnemyUnit(7,20,73,20,1), new GeneratedEnemyUnit(-15,-15,77,1,4), new GeneratedEnemyUnit(8,17,24,37,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "02cc6b72d8338a10c42e913434be1e4e282be8cb21a8a0072204a38ac8e0e0b3");
        }

        private static void Case_04157()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4157,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,20,44,4,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "5a1c2d43e5e212d0f4a1d4b87f2d71f3dd4a90e7ad456f69e15c5fb6bc4d803b");
        }

        private static void Case_04158()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4158,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-4,17,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-2,24,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-1,67,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-16,13,36,4), new GeneratedEnemyUnit(-5,-16,18,37,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "d2b44d9a80f9d7ea6cd6a5ecfdc3fef4d2f904887acd6b44190ef68aabab68f7");
        }

        private static void Case_04159()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4159,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-17,18,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-11,98,11,1), new GeneratedEnemyUnit(-6,4,20,26,4), new GeneratedEnemyUnit(17,18,51,37,4), new GeneratedEnemyUnit(-8,17,49,10,4), new GeneratedEnemyUnit(9,-4,30,27,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "c953f6c6ee1d228a11b1b192db31fae4f088d30575296c356b85364c2dc8eae2");
        }

        private static void Case_04160()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4160,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,9,62,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,6,63,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-12,41,23,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "311061747e43d937e4b5fd8d4dfd4b63e470b35f5eae22573b1915a7f447db05");
        }

        private static void Case_04161()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4161,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,15,75,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,5,24,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,2,66,50,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "13e64b9bf0a784fc44d7ffa3cd672cfa4c53d83ffd0c2fa035cb55b734db95df");
        }

        private static void Case_04162()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4162,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-5,84,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,7,31,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,10,66,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-18,11,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,11,59,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,2,93,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,13,40,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-15,98,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,9,20,8,1), new GeneratedEnemyUnit(9,-8,62,32,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "f49ef502e4d49b1ce54eaa800447bb540d3ed07d4c962c01e8d8082610993205");
        }

        private static void Case_04163()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4163,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-20,85,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,1,46,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,4,40,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-20,83,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-5,63,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,4,33,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-11,48,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,4,35,7,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "181c9fc50d462c4d329e0686692711f4f3051851e97c591487a48024bde92e96");
        }

        private static void Case_04164()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4164,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-6,53,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,9,61,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-3,44,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-6,22,47,2), new GeneratedEnemyUnit(6,4,23,38,3), new GeneratedEnemyUnit(3,-11,88,41,3), new GeneratedEnemyUnit(-13,-8,51,8,3), new GeneratedEnemyUnit(-17,-15,74,27,1), new GeneratedEnemyUnit(-13,5,94,2,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "683447d2ac29e2faf96a27a23f065f4e589551970d2607ba6905f95b8f147c8c");
        }

        private static void Case_04165()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4165,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-17,75,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-11,81,41,4), new GeneratedEnemyUnit(11,17,70,36,3), new GeneratedEnemyUnit(-3,-7,67,10,1), new GeneratedEnemyUnit(19,-18,99,19,4), new GeneratedEnemyUnit(19,1,27,34,4), new GeneratedEnemyUnit(-9,9,60,14,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "b387ffcf12ca490ac9c503b84232684a03b3747626a1c6ce8f5632576ee604c2");
        }

        private static void Case_04166()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4166,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-16,8,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,2,82,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,11,82,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,16,88,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-6,49,15,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 75,
                stableHash: "8805ab1ac5ec4872f4c303a473eef32fd21818556b6b6b5dcc4ad4f05b91ce9a");
        }

        private static void Case_04167()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4167,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-14,82,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,6,85,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-2,95,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,14,82,23,2), new GeneratedEnemyUnit(-10,-19,43,24,2), new GeneratedEnemyUnit(20,-19,29,22,3), new GeneratedEnemyUnit(-18,-1,78,48,1), new GeneratedEnemyUnit(1,-14,79,43,3), new GeneratedEnemyUnit(-20,-6,6,44,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 45,
                stableHash: "00ff00dd1fb0cee630885a544dcd671b19cd80e6aa3a240542eb51e44ac1c042");
        }

        private static void Case_04168()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4168,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-14,33,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,11,6,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,19,57,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,2,46,3,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "fc8e87547cb10f4052a6d5fbe42c8ec237c31cb379f2d48fa0c9336f01c51c3e");
        }

        private static void Case_04169()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4169,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,2,35,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-18,14,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-19,62,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-13,53,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,14,77,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,8,12,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-13,68,40,4), new GeneratedEnemyUnit(4,-17,74,20,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "039995d07a438388fb5194fe3a7c1a4d56ce957737be51e1ce7795b3f2d46b5e");
        }

        private static void Case_04170()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4170,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,17,83,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-16,32,41,4), new GeneratedEnemyUnit(-19,-19,28,10,1), new GeneratedEnemyUnit(-6,14,80,20,2), new GeneratedEnemyUnit(-8,-15,42,34,3), new GeneratedEnemyUnit(-8,20,77,1,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "ce09b22b9390d365a68610afafec8cedaa0198c3076ab06c112119704ee0a281");
        }

        private static void Case_04171()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4171,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-2,29,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-10,84,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-10,79,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-17,19,45,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "e6c25a627b02e2e02fec6492828df71e44d3c4b67a2b2351a53805e642b66be8");
        }

        private static void Case_04172()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4172,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,12,50,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,2,18,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,4,59,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,11,53,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,7,48,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,8,16,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,19,90,26,1), new GeneratedEnemyUnit(-8,-13,47,42,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "7a8a7dd7d1691ad72b9bb784a3adc0f5d85f668b26393f6f5ee437df75e3a885");
        }

        private static void Case_04173()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4173,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,8,32,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,18,25,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-18,77,34,3), new GeneratedEnemyUnit(13,5,15,20,3), new GeneratedEnemyUnit(-6,2,96,25,2), new GeneratedEnemyUnit(3,-18,99,48,3), new GeneratedEnemyUnit(-20,8,16,49,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 35,
                stableHash: "4bcd0a4457b705da781da163b96d6e8fca89556de08ab83efa6b3b70339b7ac3");
        }

        private static void Case_04174()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4174,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,12,9,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,18,77,35,2), new GeneratedEnemyUnit(-9,-6,25,50,4), new GeneratedEnemyUnit(1,0,93,24,1), new GeneratedEnemyUnit(-5,16,68,5,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "b7efc3971d4a135ec38c7b4c912211e90bffbb5fb2f84eb410db68cbc521c52f");
        }

        private static void Case_04175()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4175,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-18,59,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-4,40,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,3,44,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,4,47,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,12,72,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,17,26,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-10,41,6,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,18,74,43,1), new GeneratedEnemyUnit(10,-16,34,5,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "25ad90534b28d7793d24e79ba5212398bb92467e755f66389b1ded792e68d595");
        }

        private static void Case_04176()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4176,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-10,12,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-17,68,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-12,12,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-2,43,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,4,87,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-2,92,43,4), new GeneratedEnemyUnit(5,20,30,3,1), new GeneratedEnemyUnit(0,5,36,1,4), new GeneratedEnemyUnit(-12,-20,85,5,1), new GeneratedEnemyUnit(8,18,95,29,4), new GeneratedEnemyUnit(-15,-20,66,43,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "2215fb2c38c25ebdab0f3a241442afd0b28b4b25505bc3c1cac123253beddc77");
        }

        private static void Case_04177()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4177,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,8,18,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-14,83,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,13,30,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,18,7,27,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "bf9d99d98265e12a467b2a2f92fdbdd4e48ae2ce1daba917f60695c204a3958d");
        }

        private static void Case_04178()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4178,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,11,31,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-20,68,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-19,57,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-3,63,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-1,94,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-17,95,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-11,100,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,12,38,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,11,66,47,4), new GeneratedEnemyUnit(-16,-8,35,14,3), new GeneratedEnemyUnit(3,13,46,22,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "bacafb5071ca60c4fb140f8726cc0112c13b919e9dd055195eee5d026cd86ed0");
        }

        private static void Case_04179()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4179,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-10,17,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-2,86,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-11,80,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,17,24,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,19,51,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-7,28,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-10,78,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,19,93,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,10,32,38,2), new GeneratedEnemyUnit(0,16,7,3,1), new GeneratedEnemyUnit(-2,-17,92,19,3), new GeneratedEnemyUnit(19,14,66,19,3), new GeneratedEnemyUnit(-1,-10,7,29,4), new GeneratedEnemyUnit(-12,-19,16,12,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "4ce1921bc3ed9fb8cd7511857b11043055b7d89247ac3197590b17ed5007aa02");
        }

        private static void Case_04180()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4180,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,20,74,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-19,85,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-10,67,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,5,49,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,7,55,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-15,85,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,4,77,33,2), new GeneratedEnemyUnit(-9,15,53,20,1), new GeneratedEnemyUnit(-19,1,41,50,1), new GeneratedEnemyUnit(2,-19,78,47,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "2406ccbb9332fff8c2329b962e7a292045dad3bb4d212a7d42da051543a9dabd");
        }

        private static void Case_04181()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4181,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,6,89,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,6,35,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-4,14,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,1,88,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-14,9,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-13,25,44,1), new GeneratedEnemyUnit(-6,3,28,2,1), new GeneratedEnemyUnit(9,16,54,11,2), new GeneratedEnemyUnit(-11,-9,91,8,3), new GeneratedEnemyUnit(-3,5,46,17,3), new GeneratedEnemyUnit(-3,0,11,5,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "4a07d0a4b7f41f5ac3c5e4ef5f8174daba3e5944c64267f1e90fe47faec26396");
        }

        private static void Case_04182()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4182,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-8,63,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-4,18,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,3,53,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,4,79,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,19,60,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,2,15,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-16,60,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,5,77,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,8,47,6,2), new GeneratedEnemyUnit(-19,20,24,19,4), new GeneratedEnemyUnit(-13,-2,73,27,4), new GeneratedEnemyUnit(-13,-10,27,40,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "92feff421f47c6bb145e3e17e4e4e3581e10127f87b0283a6488c4717e095968");
        }

        private static void Case_04183()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4183,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-9,81,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,12,25,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-16,7,23,3), new GeneratedEnemyUnit(2,-1,12,17,4), new GeneratedEnemyUnit(-15,4,65,8,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "3c857ff5b26f8e41fc667d3f44689ce5ee124280293b30077e82e29b69f085d0");
        }

        private static void Case_04184()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4184,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-6,92,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-18,63,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-2,45,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,17,55,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,17,47,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-17,92,11,1), new GeneratedEnemyUnit(14,4,16,49,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "789811f77ce5c6fc7de993cfe590b5831eab5ba606c60130e1e046f26b7c170b");
        }

        private static void Case_04185()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4185,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,16,53,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,15,50,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-12,25,13,4), new GeneratedEnemyUnit(-8,-3,59,47,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "c7413a6f2b4ee5dd77ff045fe377f105ef10a2ae36aeb7acb4d42ba7426cbadd");
        }

        private static void Case_04186()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4186,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,15,7,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-1,48,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,15,83,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,7,47,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,19,19,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,10,61,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-10,6,22,1), new GeneratedEnemyUnit(18,18,92,37,1), new GeneratedEnemyUnit(10,-4,82,24,4), new GeneratedEnemyUnit(0,9,99,47,4), new GeneratedEnemyUnit(-4,17,20,29,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "64d3736d8b7e8fd6d9ff07a894b8060dfca96d49c6b0fdd3423d1914e43d3e91");
        }

        private static void Case_04187()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4187,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,11,81,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,12,98,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-10,55,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,1,90,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,16,23,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,16,94,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,4,20,6,4), new GeneratedEnemyUnit(19,14,44,27,1), new GeneratedEnemyUnit(14,-16,69,40,4), new GeneratedEnemyUnit(-13,-6,12,5,4), new GeneratedEnemyUnit(17,15,58,43,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "2c745c8317a9a3bc6f6a3ff0c150282aa07aeb53737193e64a16f9439ac9b33e");
        }

        private static void Case_04188()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4188,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,6,82,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,4,39,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-8,83,39,4), new GeneratedEnemyUnit(-13,2,47,42,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 25,
                stableHash: "da0237c044294cc786c1fd4bf74565af1bdd14b662774320224474f25b64e9f9");
        }

        private static void Case_04189()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4189,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,11,14,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,2,52,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-1,86,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,4,63,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-12,68,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,9,34,24,3), new GeneratedEnemyUnit(4,8,68,2,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "fdc6f31b716d5eeabb66da66e8e9bcd2b46ea901ef832f6bd8567f0dde03e6c8");
        }

        private static void Case_04190()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4190,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,8,55,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,5,79,24,4), new GeneratedEnemyUnit(13,-15,10,23,4), new GeneratedEnemyUnit(12,2,61,43,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "40dc72d05419d0e31578af946e176203741b851b1a8734319e49516964bcc587");
        }

        private static void Case_04191()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4191,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-2,39,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,16,22,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-16,77,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-18,54,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-3,43,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,2,6,8,2), new GeneratedEnemyUnit(6,12,50,44,2), new GeneratedEnemyUnit(-8,6,29,6,4), new GeneratedEnemyUnit(-2,-13,74,12,3), new GeneratedEnemyUnit(19,17,31,29,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "03c54e1ad0eeaba300dc8fdcb9dee7220686aebad9da8bcb2013ef63dd4bd199");
        }

        private static void Case_04192()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4192,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-11,53,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-7,59,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,4,93,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,19,44,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-14,90,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-14,7,9,1), new GeneratedEnemyUnit(-16,-7,69,8,4), new GeneratedEnemyUnit(20,-7,67,36,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "1721f1b1e00f7380f452e3816ea206a7d732f2b00ca7d33a1d4587cbe2c3c69a");
        }

        private static void Case_04193()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4193,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-13,17,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,16,19,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,12,22,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-4,96,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,12,26,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,12,45,41,1), new GeneratedEnemyUnit(3,5,20,38,3), new GeneratedEnemyUnit(11,1,77,26,1), new GeneratedEnemyUnit(-14,-20,88,10,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "c269f6668b487d276d0951ed51e77ee7cadf4add3716d99246459ea73d82595f");
        }

        private static void Case_04194()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4194,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,3,94,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-4,51,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-10,58,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-13,40,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-6,15,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-4,8,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-5,32,6,4), new GeneratedEnemyUnit(-13,12,5,2,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "94ec806e0b661d8802eaadcad90acca702f9a50d41c875284ac9af6bad0c3939");
        }

        private static void Case_04195()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4195,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,13,100,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,7,45,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-15,23,7,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,8,42,39,1), new GeneratedEnemyUnit(11,-2,20,49,3), new GeneratedEnemyUnit(17,1,92,29,2), new GeneratedEnemyUnit(-8,-12,31,39,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "4bfd01ef281b7479a25c6cce3993a0afcc7f8bb95794b14d9ad9ec60ea75e911");
        }

        private static void Case_04196()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4196,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,17,38,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,11,98,4,3), new GeneratedEnemyUnit(-2,14,68,9,4), new GeneratedEnemyUnit(-14,-9,47,24,1), new GeneratedEnemyUnit(1,18,34,12,3), new GeneratedEnemyUnit(5,14,63,26,2), new GeneratedEnemyUnit(4,-2,58,8,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "6055172ee16820170473f8b591eda5f279b9d72ee419353e8a1d461671ef7967");
        }

        private static void Case_04197()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4197,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-3,81,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-20,75,26,1), new GeneratedEnemyUnit(-5,-15,70,11,1), new GeneratedEnemyUnit(-3,-17,62,17,4), new GeneratedEnemyUnit(19,2,60,48,3), new GeneratedEnemyUnit(6,12,66,45,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "1ba833cafcde981f0b86ebf4386b5e079eba581d743721d1e5206a0a19df6419");
        }

        private static void Case_04198()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4198,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-3,96,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-1,50,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,6,67,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,15,53,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,7,62,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,8,97,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-11,57,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-7,36,12,2), new GeneratedEnemyUnit(4,17,64,8,4), new GeneratedEnemyUnit(-5,11,41,40,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "50ddc18b63b3f96bb9d7866cf842eae75ecda985af6b2e193fb422c851c45002");
        }

        private static void Case_04199()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4199,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-7,63,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,14,89,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,15,17,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-20,97,31,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "2fb6684239205c05abce052d6a85854d8204adf17644e9a720fc3847add0daa1");
        }

        private static void Case_04200()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4200,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,8,5,1,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "a949ea6e12365c6700943e059cd04043cafdcfeee4853e040bb11e457c114da6");
        }

        private static void Case_04201()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4201,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,8,6,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,11,87,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,10,28,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-14,37,46,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "2e702bcae191308640d69da2a3d7c1e87da3226d949967d22c295bc02e340cd2");
        }

        private static void Case_04202()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4202,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,0,43,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-19,22,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,12,49,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-18,42,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-3,78,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,6,46,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,14,43,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,4,31,41,2), new GeneratedEnemyUnit(-15,-6,64,43,4), new GeneratedEnemyUnit(-8,-8,34,7,2), new GeneratedEnemyUnit(6,13,37,38,1), new GeneratedEnemyUnit(12,18,14,29,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "2ddfe57af5557bfbcb5a4811d0ad092b86603c4aabe97742039684d8724f2c51");
        }

        private static void Case_04203()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4203,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-9,7,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-13,54,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,17,28,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-11,26,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-19,89,24,1), new GeneratedEnemyUnit(-4,4,65,25,3), new GeneratedEnemyUnit(-16,9,67,7,1), new GeneratedEnemyUnit(15,1,27,42,3), new GeneratedEnemyUnit(1,-11,17,11,3), new GeneratedEnemyUnit(-1,-19,72,24,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "93c271ab8492994d4d2182d3c505066578337fe55032547e939070b48f22e142");
        }

        private static void Case_04204()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4204,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-4,33,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,20,41,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,7,8,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-6,48,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-10,16,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,10,63,3,1), new GeneratedEnemyUnit(19,1,74,36,2), new GeneratedEnemyUnit(-6,-18,89,34,4), new GeneratedEnemyUnit(19,18,95,42,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "ba5546b3c11d001082dbcef1305d20ec724c64bf0f171ad40d008dba86c1e39c");
        }

        private static void Case_04205()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4205,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-4,59,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-10,69,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,7,100,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,11,6,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,8,70,45,2), new GeneratedEnemyUnit(12,-1,19,45,2), new GeneratedEnemyUnit(19,-9,33,9,1), new GeneratedEnemyUnit(18,-2,88,4,4), new GeneratedEnemyUnit(2,18,12,43,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "c1f10ee71a14b8c202629f1f74444e7481408960b5c65e6ab366ca27d0212de8");
        }

        private static void Case_04206()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4206,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,5,36,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,20,72,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-9,47,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,7,8,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,16,96,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,8,45,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-7,63,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,2,64,7,2), new GeneratedEnemyUnit(1,3,22,5,3), new GeneratedEnemyUnit(5,6,54,6,2), new GeneratedEnemyUnit(14,5,80,2,3), new GeneratedEnemyUnit(13,-18,24,40,1), new GeneratedEnemyUnit(-13,-7,34,26,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "76a73749227e9bef8af8401be27f862ef1198e6f2144e5f2e698c78a84540692");
        }

        private static void Case_04207()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4207,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-6,62,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-18,70,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-2,9,20,1), new GeneratedEnemyUnit(12,19,89,18,4), new GeneratedEnemyUnit(-1,-7,81,39,1), new GeneratedEnemyUnit(2,4,46,7,3), new GeneratedEnemyUnit(-7,-6,30,40,4), new GeneratedEnemyUnit(8,-4,14,28,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "672e93baa4c977ae3e1b926fa2d99ac3936eb5879204a61c0a2bd6763eaa9e5a");
        }

        private static void Case_04208()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4208,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,7,50,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,16,100,47,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "815c5d3d10ad008562cc1f7d5a944dda76ad7e92c41c1dcbebe83a837a870942");
        }

        private static void Case_04209()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4209,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,15,97,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-11,39,50,3), new GeneratedEnemyUnit(11,2,88,45,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "b5a4ecbcae92ca6bc9a0b1e8df82bc8190d4b6fd0ec7ad09f21c006793d30e7b");
        }

        private static void Case_04210()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4210,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-14,95,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-20,53,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,17,46,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,18,67,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-20,43,5,4), new GeneratedEnemyUnit(17,-18,97,14,2), new GeneratedEnemyUnit(2,-13,18,31,3), new GeneratedEnemyUnit(3,-12,85,8,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "0511ded5cb9ba8689c72524275054863fb74f0797e0b9c173f8a06ab03c129b3");
        }

        private static void Case_04211()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4211,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-14,23,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,4,66,31,4), new GeneratedEnemyUnit(15,9,67,48,4), new GeneratedEnemyUnit(-9,9,46,38,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "2b7765a1003c0e21ab6af61cafa722a9494a1da0a73f7fdedaddd99c55dc29a8");
        }

        private static void Case_04212()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4212,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-19,58,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-6,76,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,1,24,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-17,37,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-11,26,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-6,74,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,11,10,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-2,13,1,3), new GeneratedEnemyUnit(16,13,42,11,2), new GeneratedEnemyUnit(-7,20,34,44,3), new GeneratedEnemyUnit(7,-6,15,20,4), new GeneratedEnemyUnit(-8,10,98,37,1), new GeneratedEnemyUnit(-1,13,52,7,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "4f350d9e2a1fcca657f0a539cea0a81400c63a89f9caec9cf709e66e0a91a579");
        }

        private static void Case_04213()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4213,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,7,80,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,16,69,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-3,66,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-10,37,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,6,78,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,3,8,38,2), new GeneratedEnemyUnit(-18,-18,36,3,3), new GeneratedEnemyUnit(15,-20,90,3,2), new GeneratedEnemyUnit(-9,6,63,7,1), new GeneratedEnemyUnit(-12,-6,77,27,4), new GeneratedEnemyUnit(9,12,88,10,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "9a83de5351211f57fafbca6728aa8e411947cbca0ec8d598668efcfb9b1cf980");
        }

        private static void Case_04214()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4214,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,2,34,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,10,88,15,2), new GeneratedEnemyUnit(-17,-10,52,26,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "f7b6be3edfbed9b5acdeace083b613bf35bc0611af210764c8d88d4dfaf365d5");
        }

        private static void Case_04215()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4215,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,19,53,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,13,70,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-20,64,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,6,45,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-2,12,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-11,67,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-5,29,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-9,9,45,4), new GeneratedEnemyUnit(6,-20,93,20,3), new GeneratedEnemyUnit(10,18,42,26,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "f4eea3f0c758751b84a16c8f59a0fdbd0f3e0b44c656cbdd52a3b542964f5217");
        }

        private static void Case_04216()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4216,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-3,43,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,11,63,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-3,22,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,10,86,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-13,61,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,9,84,39,3), new GeneratedEnemyUnit(-14,-8,30,5,2), new GeneratedEnemyUnit(10,6,70,30,2), new GeneratedEnemyUnit(-5,-8,78,45,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "b0349b65623ef41955262598554d7a5d547697fd06cc20cd8aaebb57355eb1b0");
        }

        private static void Case_04217()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4217,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-5,24,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-16,42,41,2), new GeneratedEnemyUnit(14,-6,95,31,3), new GeneratedEnemyUnit(-4,17,22,18,3), new GeneratedEnemyUnit(-11,-11,21,39,3), new GeneratedEnemyUnit(18,2,72,14,4), new GeneratedEnemyUnit(-8,9,72,14,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "86504cbfdc8feca75b24a0fd644c27f20de2511a67c1c19e6903c18d985a0ff0");
        }

        private static void Case_04218()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4218,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,8,63,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,15,66,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,4,52,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,0,66,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-12,47,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,3,6,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-10,26,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,16,62,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,10,66,20,4), new GeneratedEnemyUnit(-6,19,18,10,4), new GeneratedEnemyUnit(-5,1,55,19,4), new GeneratedEnemyUnit(7,-9,12,50,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "fea981521ee35a5d5d9121cd0841c825a9b31cbe8584e50a12d4ca7436d8cc9b");
        }

        private static void Case_04219()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4219,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,4,64,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,2,16,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,8,79,8,3), new GeneratedEnemyUnit(-2,-9,20,28,3), new GeneratedEnemyUnit(12,-20,47,48,1), new GeneratedEnemyUnit(-2,16,80,42,4), new GeneratedEnemyUnit(10,19,48,36,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "5d160e8f0a5db518ef29e74385a2e8b13baf8d7ffd2515d323c44c3b8c2f8725");
        }

        private static void Case_04220()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4220,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,0,9,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-11,91,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-18,98,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-15,79,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,10,83,22,3), new GeneratedEnemyUnit(-10,17,31,26,4), new GeneratedEnemyUnit(-3,7,88,1,4), new GeneratedEnemyUnit(-18,-14,20,29,1), new GeneratedEnemyUnit(-2,-13,90,32,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "9c4a32d737429c4f6adc6daeb9f4176262bab49bdd81e609ff6da8bf34ea1b90");
        }

        private static void Case_04221()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4221,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-14,86,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,11,64,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,7,39,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,10,45,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,20,26,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-5,7,37,3), new GeneratedEnemyUnit(-6,14,7,30,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "6df11b324f3caff8def5c1416514a39ffe1ab338ca308f7d333a83531eec3d99");
        }

        private static void Case_04222()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4222,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,17,84,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-8,51,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-3,95,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-15,85,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-2,76,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,0,80,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,2,41,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,7,84,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-17,46,31,3), new GeneratedEnemyUnit(11,15,49,24,2), new GeneratedEnemyUnit(-12,-2,31,7,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "145b7d41f03c5f1888905b8c1a8dcedf06cfe5a689b95ddd4038239434595340");
        }

        private static void Case_04223()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4223,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-4,94,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-6,50,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-4,44,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,12,32,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,0,79,29,2), new GeneratedEnemyUnit(19,-9,38,30,3), new GeneratedEnemyUnit(7,-13,39,17,4), new GeneratedEnemyUnit(-13,8,43,8,1), new GeneratedEnemyUnit(17,2,67,33,1), new GeneratedEnemyUnit(-17,-10,10,9,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "fd71cf0989ba1117161da2321c807b06075d5cd2af360d541021992d69ee7cca");
        }

        private static void Case_04224()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4224,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,18,56,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,1,24,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-19,76,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-15,35,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,7,22,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,6,85,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,13,94,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,20,72,50,2), new GeneratedEnemyUnit(-19,11,60,44,4), new GeneratedEnemyUnit(-9,-19,45,32,3), new GeneratedEnemyUnit(-14,3,51,18,3), new GeneratedEnemyUnit(12,11,45,3,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "1990ebc96563d2f42d269371bda409c0f87146d883ee1c4b0cfdd66fcb6daa9b");
        }

        private static void Case_04225()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4225,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-9,100,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,9,14,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-18,17,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,7,95,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-18,25,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,20,41,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-1,29,49,3), new GeneratedEnemyUnit(-17,2,69,16,2), new GeneratedEnemyUnit(12,-15,33,44,1), new GeneratedEnemyUnit(-12,6,43,41,4), new GeneratedEnemyUnit(20,-16,27,32,3), new GeneratedEnemyUnit(8,-6,5,48,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 40,
                stableHash: "7cf19b8aadbb714ce9b378bc3cb714d4a2d00472b6cd192e7f03e2b74e915952");
        }

        private static void Case_04226()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4226,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,13,55,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-1,15,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-6,19,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-9,72,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,15,95,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,8,97,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,2,60,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,0,98,16,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "c7959543c7eeea5f1bf6f73a68cca8b6fdd760300c54f3e50213e4a83adeb5ac");
        }

        private static void Case_04227()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4227,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-2,32,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,8,31,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-15,62,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,4,95,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-11,68,32,2), new GeneratedEnemyUnit(4,16,73,27,1), new GeneratedEnemyUnit(-1,-16,17,26,4), new GeneratedEnemyUnit(-8,-1,23,21,3), new GeneratedEnemyUnit(10,6,63,15,3), new GeneratedEnemyUnit(3,7,10,42,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 45,
                stableHash: "c21ed7e49c1a2465c701661d6c656c9fb3f8cdfe99579c9ba932537de017af37");
        }

        private static void Case_04228()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4228,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,14,67,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,7,25,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,3,60,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,16,52,44,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "3a047aa9e99dd37f5ccb7d35fa982519dd2f511ddead5eb47f0fe3cb5d362cf9");
        }

        private static void Case_04229()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4229,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,0,68,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,11,52,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,6,54,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-16,23,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-17,47,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,2,10,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-3,55,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-5,92,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,3,78,1,3), new GeneratedEnemyUnit(6,20,78,48,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "291993b1b5fd6eb2c24bdfb05c72d7f3307f7dc354b8d8065cf260a3d6148662");
        }

        private static void Case_04230()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4230,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-20,79,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,0,56,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,4,9,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-8,7,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-2,73,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-20,56,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-9,11,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-11,71,4,4), new GeneratedEnemyUnit(-7,5,100,22,4), new GeneratedEnemyUnit(6,-16,90,15,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "6e115caf81b2db383ae98f124c105d946ab74afe83c147851c48457cad3756e0");
        }

        private static void Case_04231()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4231,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,18,13,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,16,67,32,3), new GeneratedEnemyUnit(-14,0,75,28,4), new GeneratedEnemyUnit(6,3,67,6,1), new GeneratedEnemyUnit(-2,11,69,38,4), new GeneratedEnemyUnit(-1,-14,28,18,3), new GeneratedEnemyUnit(-15,-18,7,50,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 100,
                stableHash: "afd8d445235d2510f6fbcee157985884c869a0ef21d6cf5b407f4432c42f073f");
        }

        private static void Case_04232()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4232,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,1,24,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,8,32,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,15,31,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-3,29,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-18,35,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,9,16,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,15,16,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-3,19,18,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "8b404edbdd0eae211c919fd1861feb41cec167a1f10fb6e09fa2e50d600d88ee");
        }

        private static void Case_04233()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4233,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-5,89,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,6,35,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-12,25,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-15,11,43,1), new GeneratedEnemyUnit(-2,-13,66,24,2), new GeneratedEnemyUnit(-10,13,9,46,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "b8f29a06c5219919166d2bb15e7346767ca657ec395af4dd83ffebf1b8d3bf49");
        }

        private static void Case_04234()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4234,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-12,49,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-19,32,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,0,62,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,18,20,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,16,8,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,15,20,13,2), new GeneratedEnemyUnit(-16,11,21,22,3), new GeneratedEnemyUnit(-12,8,69,39,1), new GeneratedEnemyUnit(11,8,58,34,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "2c5347284b361005ef3b4d0c1b99b597a448983818196a87f7c5694d87d74eb2");
        }

        private static void Case_04235()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4235,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-11,31,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,1,37,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,16,6,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-15,33,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-20,19,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,11,46,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-12,75,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-14,75,1,1), new GeneratedEnemyUnit(19,14,30,23,3), new GeneratedEnemyUnit(11,19,76,20,1), new GeneratedEnemyUnit(-9,-1,42,10,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "00e748f4d011f793d7541c80e1eda3aa10d4f414284936c5d837dfb51edd6dd1");
        }

        private static void Case_04236()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4236,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-6,43,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,8,21,20,3), new GeneratedEnemyUnit(5,9,96,6,3), new GeneratedEnemyUnit(15,10,29,14,2), new GeneratedEnemyUnit(17,-19,16,18,4), new GeneratedEnemyUnit(-8,2,26,43,1), new GeneratedEnemyUnit(-10,-16,63,40,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "5e52fb68c97dcf21888b0d30f965f1cf31d6ea4dca9d1658a5fa2ff7cfcbf034");
        }

        private static void Case_04237()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4237,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,17,88,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,11,36,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,6,57,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-11,97,35,3), new GeneratedEnemyUnit(11,-13,92,5,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "5a0d6da0cb4d8875c2f4776d5f23bdd56cf4c91c8330b20d69bd86ae3491de7e");
        }

        private static void Case_04238()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4238,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,17,55,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-3,68,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,10,5,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,17,70,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-12,61,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,10,98,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-17,23,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-11,79,21,4), new GeneratedEnemyUnit(4,-14,30,15,2), new GeneratedEnemyUnit(6,7,43,31,3), new GeneratedEnemyUnit(11,14,95,49,4), new GeneratedEnemyUnit(-10,-11,48,11,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "018af18293b8e8066afe1d5985170238b3db6235cf011bb2c69a11efc2ef4547");
        }

        private static void Case_04239()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4239,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,1,8,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-11,22,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-2,5,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,1,80,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-5,48,8,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "43a6ee562dd28bc6c94ae0d6ad06a186af05e8feebdad3faa7ed2bdddd2837c8");
        }

        private static void Case_04240()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4240,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-6,48,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,5,32,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,20,14,12,3), new GeneratedEnemyUnit(-5,-18,58,4,3), new GeneratedEnemyUnit(-15,-14,15,14,4), new GeneratedEnemyUnit(1,-20,61,45,3), new GeneratedEnemyUnit(-15,-10,20,32,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "0de65c0193fdb11eb250d4fe395e08a67afedeaac244db08f14fd86b9fdc50f8");
        }

        private static void Case_04241()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4241,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-13,74,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-9,26,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,5,52,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,18,100,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-12,66,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,11,41,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-7,76,32,3), new GeneratedEnemyUnit(-3,18,38,17,1), new GeneratedEnemyUnit(11,13,83,2,3), new GeneratedEnemyUnit(-4,-16,98,40,3), new GeneratedEnemyUnit(18,-20,99,36,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "210ec6fdf8c0417fb255f41fe43359a0d515ad08cb91c765180242d50d20de64");
        }

        private static void Case_04242()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4242,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-13,79,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-5,7,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-3,64,5,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "f303dbb7cf2c5d0b6cc2a9e8374a4380b388dc2962df76d5dbdee7148c37feea");
        }

        private static void Case_04243()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4243,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-7,33,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-16,60,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-4,89,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-1,67,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,2,52,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-1,26,27,1), new GeneratedEnemyUnit(-20,-1,50,15,3), new GeneratedEnemyUnit(5,20,79,19,3), new GeneratedEnemyUnit(0,-8,21,22,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "fd7892db70a1f51127e648e48206af3af4d4018f1e626010b2575df05923173f");
        }

        private static void Case_04244()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4244,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,17,85,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-15,17,8,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "a63c7e1dca23ebe97ca8624e2a1552f85f3cd197c4e636727cc4825d0ecfe54e");
        }

        private static void Case_04245()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4245,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,0,20,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,2,97,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,12,85,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-2,50,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,16,77,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,6,41,5,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "4f452f3a7d09483e0e4a3ec53c56290d9ecf207c14a119bf0eaf16a262b07011");
        }

        private static void Case_04246()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4246,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-4,94,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,1,28,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,8,81,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-1,49,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,20,37,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-19,88,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,3,43,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-14,85,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,1,99,31,1), new GeneratedEnemyUnit(5,1,56,26,2), new GeneratedEnemyUnit(16,-11,80,33,4), new GeneratedEnemyUnit(20,-5,54,12,2), new GeneratedEnemyUnit(2,16,36,39,4), new GeneratedEnemyUnit(19,1,95,46,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "cd1b50781b40e8a8677aebde0494171a2393447f31b6116dedb1d8383fdc77de");
        }

        private static void Case_04247()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4247,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,20,61,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-5,74,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-12,87,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,3,83,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-20,87,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-4,13,33,1), new GeneratedEnemyUnit(-10,2,80,9,2), new GeneratedEnemyUnit(10,9,80,37,4), new GeneratedEnemyUnit(-8,-11,97,37,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "701f879cb22e90e1f390acc6d6de8e544c6efc4cef226b3c0fc651ee74dd986e");
        }

        private static void Case_04248()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4248,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,4,99,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,5,78,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-12,55,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-12,13,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-15,94,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-9,29,6,4), new GeneratedEnemyUnit(2,16,54,4,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "d23a3bed12e8a872893c3d5e045d70446e720b62f074ceda194b83af0f1a6337");
        }

        private static void Case_04249()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4249,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,7,67,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,12,71,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-12,72,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,18,32,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-13,74,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,5,53,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-17,46,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-12,49,23,1), new GeneratedEnemyUnit(-17,-17,78,13,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "67e5a69c908cd15c53c9b71f5d38adf40d135876c4863e95f64bb0e937a2f9ce");
        }

        private static void Case_04250()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4250,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,0,81,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-4,76,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,16,18,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,20,23,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-3,42,11,4), new GeneratedEnemyUnit(8,1,11,46,3), new GeneratedEnemyUnit(20,-11,52,24,1), new GeneratedEnemyUnit(3,1,100,4,1), new GeneratedEnemyUnit(-1,0,100,21,4), new GeneratedEnemyUnit(-18,-8,88,44,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "cda069a7d0b6836520d650796395dd4f00905321e2a71577e571c4eef02794be");
        }

        private static void Case_04251()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4251,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-15,22,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-2,5,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-1,45,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,15,15,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,0,7,48,1), new GeneratedEnemyUnit(16,17,90,25,4), new GeneratedEnemyUnit(-11,-13,99,35,3), new GeneratedEnemyUnit(12,-3,97,5,3), new GeneratedEnemyUnit(-6,-9,47,21,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "ce8ecf306b798416572b2048df378badcdfa7f8286c919be8b27d2f60ff42115");
        }

        private static void Case_04252()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4252,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-3,71,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,16,69,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-1,55,4,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "0f7c86172a5d4e3ba1992f32b51b78e37a7d74182ab3f5c49ae6d1b87f102a27");
        }

        private static void Case_04253()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4253,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-4,84,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-13,100,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-4,80,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,1,89,25,2), new GeneratedEnemyUnit(-20,19,97,13,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "fd7aa12cf6451cc8e84e6de8b561a3642a214e483db97bb16bff3f7ee38e86b1");
        }

        private static void Case_04254()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4254,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,2,32,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,13,78,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,17,15,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-20,85,17,1), new GeneratedEnemyUnit(-5,6,16,44,3), new GeneratedEnemyUnit(-5,11,49,37,2), new GeneratedEnemyUnit(-2,15,61,12,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "0df3bee146ff0371d0f3e0e0ce2e303c94af3401339b6fb87f3effd4a5ce5043");
        }

        private static void Case_04255()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4255,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,12,21,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,9,29,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,12,73,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,15,61,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-5,11,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,3,41,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-11,47,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-8,38,36,3), new GeneratedEnemyUnit(-2,-12,32,13,3), new GeneratedEnemyUnit(-20,3,80,2,1), new GeneratedEnemyUnit(10,12,37,35,1), new GeneratedEnemyUnit(-1,-12,21,28,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "5605479596ba61f2a92f656b47a467f6d63aabb19bb02a5c651e16a3a6bf8e75");
        }

        private static void Case_04256()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4256,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,12,74,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,20,57,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,0,16,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-9,23,5,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,11,100,6,2), new GeneratedEnemyUnit(9,-18,18,9,4), new GeneratedEnemyUnit(-11,-20,54,29,2), new GeneratedEnemyUnit(8,13,68,29,1), new GeneratedEnemyUnit(14,-14,63,2,2), new GeneratedEnemyUnit(-20,2,61,21,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "e4a649d9784c61a71e5bcfb9f5f7ba45bcb8543c1f34b91a99199702ec6f84f3");
        }

        private static void Case_04257()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4257,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-17,42,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,18,5,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,13,48,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,13,92,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,10,85,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-19,98,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,2,97,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,10,20,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,2,34,30,3), new GeneratedEnemyUnit(14,-19,57,35,2), new GeneratedEnemyUnit(-11,-10,30,30,2), new GeneratedEnemyUnit(-7,6,87,17,3), new GeneratedEnemyUnit(10,-20,5,17,4), new GeneratedEnemyUnit(-11,-6,84,17,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "8599f8a2baa2d60bdca0143a735276463dd07637a80153f536d76ac11e2e4da5");
        }

        private static void Case_04258()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4258,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,13,9,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,13,95,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-5,8,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-2,75,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-5,27,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-8,92,37,4), new GeneratedEnemyUnit(-16,-6,50,27,2), new GeneratedEnemyUnit(-18,-14,6,44,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 85,
                stableHash: "4b714d781c73f6ffed18c92a72ce913a4fa3bda18e23da8936e89d88ff27c103");
        }

        private static void Case_04259()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4259,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,4,38,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-16,76,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,18,27,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-4,78,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,6,57,3,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "cc6cdaf18f3d9f4e686fa859d483ef228db9b514a49906230874c67dd7cd55c1");
        }

        private static void Case_04260()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4260,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-13,96,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-4,93,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-14,22,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,3,79,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-4,99,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-3,71,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-6,20,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,19,19,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,15,27,50,2), new GeneratedEnemyUnit(-14,2,29,27,4), new GeneratedEnemyUnit(-2,-2,59,11,4), new GeneratedEnemyUnit(-12,-6,95,34,2), new GeneratedEnemyUnit(-10,7,57,39,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "78123700c70c1d604cf6312ece91d438666ee933cf9062f60088a3500c77f6a5");
        }

        private static void Case_04261()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4261,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,3,53,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-1,61,20,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "9292ce9d7a55c88655733a35e1eaa6cbfd9b63c67cdc487266900e106b92e7c9");
        }

        private static void Case_04262()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4262,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,12,95,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-7,17,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-20,55,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-20,99,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-16,8,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-18,42,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-5,59,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,19,35,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-5,82,43,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "34abe492458d13088e0d6bafd56aa512055d72f970c1ebaee289f5ee2b6d1eac");
        }

        private static void Case_04263()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4263,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-3,21,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-6,25,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,12,27,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-14,92,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-9,17,32,1), new GeneratedEnemyUnit(-9,4,51,50,1), new GeneratedEnemyUnit(-6,2,82,9,1), new GeneratedEnemyUnit(-13,-11,100,4,4), new GeneratedEnemyUnit(-16,11,31,34,3), new GeneratedEnemyUnit(2,-7,7,11,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "e58e3f4959f2105123d6529241ceedb5d68d833f562125a33b1a456636a34892");
        }

        private static void Case_04264()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4264,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,4,18,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-2,56,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-5,11,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-3,86,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-14,47,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,17,13,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-5,49,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,18,60,4,3), new GeneratedEnemyUnit(-14,9,93,30,2), new GeneratedEnemyUnit(-11,-11,83,2,1), new GeneratedEnemyUnit(2,17,74,8,1), new GeneratedEnemyUnit(18,16,87,4,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "550b2ebd3aa69793e14ba5102e01c3fbf783bf87e65b6c884a26f56b9e633688");
        }

        private static void Case_04265()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4265,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,17,26,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,18,75,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-18,53,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-19,39,31,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 75,
                stableHash: "37ec737a950ee213a76c687c7777b6e5aa988f647678d3517b7d6c0dc80a3de0");
        }

        private static void Case_04266()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4266,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,4,55,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-18,79,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-11,90,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,16,60,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-8,27,37,4), new GeneratedEnemyUnit(13,-15,42,22,2), new GeneratedEnemyUnit(-8,-15,10,45,4), new GeneratedEnemyUnit(7,-8,68,6,4), new GeneratedEnemyUnit(-14,-10,9,43,4), new GeneratedEnemyUnit(15,-2,47,43,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "d8a99a9d815a2f4087fc392054910c5113a672e22dce302be070119f5a104ecd");
        }

        private static void Case_04267()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4267,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,17,9,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,4,58,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,18,45,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,15,26,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-16,95,6,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "a11bdb1acc3452208031991d622e27b10a75bf6e1a0fd79353ba69f57624a4f2");
        }

        private static void Case_04268()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4268,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,18,32,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-5,71,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-9,42,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,8,44,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-19,73,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-7,63,12,1), new GeneratedEnemyUnit(-13,6,61,4,3), new GeneratedEnemyUnit(3,12,19,4,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "329f611483baee8763c8f21363940c76bde04e638d2ea0c297e459f9b6015141");
        }

        private static void Case_04269()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4269,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-10,94,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,0,56,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,12,41,35,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "4164a6dbc82a3be7de58b4d684f9c43bd66829a068a2d5782def5176118dc7e5");
        }

        private static void Case_04270()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4270,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,10,98,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-14,71,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,18,71,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-10,15,1,2), new GeneratedEnemyUnit(-1,19,56,48,1), new GeneratedEnemyUnit(13,14,51,21,1), new GeneratedEnemyUnit(4,-10,24,15,2), new GeneratedEnemyUnit(-18,6,14,17,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "3c23cf2e30398130e943f7bb345e24095fa3ec16a5f02bd1c51c144e67758350");
        }

        private static void Case_04271()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4271,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-15,16,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,9,36,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-6,37,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-1,51,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-11,89,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-6,83,26,3), new GeneratedEnemyUnit(2,1,53,15,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "4c8df9755f6ef0749b5369a351ac41593a69307e3d59aa236855a1d0ef1ab46f");
        }

        private static void Case_04272()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4272,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-20,98,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,2,65,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,9,85,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-13,51,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,5,5,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,3,58,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-5,37,25,3), new GeneratedEnemyUnit(11,6,20,4,1), new GeneratedEnemyUnit(14,6,11,46,1), new GeneratedEnemyUnit(-7,-3,96,21,1), new GeneratedEnemyUnit(5,-15,17,45,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "5d7188113e766a41d8f0902e373c3632dcb23b8c92084d69c7c007379647d89c");
        }

        private static void Case_04273()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4273,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,12,90,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-8,50,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-16,77,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-2,88,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,10,45,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-7,81,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-19,33,21,1), new GeneratedEnemyUnit(-7,-9,47,19,2), new GeneratedEnemyUnit(11,-4,22,1,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "97882cc01f942c293bcd5e7eebdd118636f079eb8124d267148c334289b05621");
        }

        private static void Case_04274()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4274,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,5,69,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,14,30,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,6,27,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,8,50,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,10,66,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,17,58,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,20,15,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-8,37,33,2), new GeneratedEnemyUnit(8,15,50,34,1), new GeneratedEnemyUnit(-4,8,67,32,2), new GeneratedEnemyUnit(7,-7,59,7,3), new GeneratedEnemyUnit(18,4,12,45,4), new GeneratedEnemyUnit(-18,12,20,10,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "4fa16873326da0dcd88dcd3c8b1329225ef61612eba85a950916263e805dc7e0");
        }

        private static void Case_04275()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4275,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,0,28,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,9,82,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,12,42,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,4,93,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-20,69,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,1,92,29,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "2f011d7fd20b7fc0c9c55e0466dcbfd15502b8fb8a3181d9ae88ccf194702036");
        }

        private static void Case_04276()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4276,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,10,8,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,2,39,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,7,80,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,16,12,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-19,60,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,16,62,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,18,81,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-14,99,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-14,68,23,3), new GeneratedEnemyUnit(-8,18,74,45,2), new GeneratedEnemyUnit(9,-20,93,3,2), new GeneratedEnemyUnit(-10,17,37,22,3), new GeneratedEnemyUnit(0,18,84,31,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "d6e7bfca7d2b2e55691d480bce7d734fe6145fbc89bffa42efee215f20f1775d");
        }

        private static void Case_04277()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4277,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,10,71,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,6,24,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,2,32,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,10,30,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-4,72,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,17,63,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-17,97,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,4,44,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-2,6,8,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "84ec95c7fa8a9271e5b0491dd34bb2d45bce5666d07dbbdc50b156b8f59378cf");
        }

        private static void Case_04278()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4278,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,18,95,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,20,10,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-4,56,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,3,49,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,17,68,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,1,94,37,4), new GeneratedEnemyUnit(-19,-9,65,31,4), new GeneratedEnemyUnit(17,-8,20,49,4), new GeneratedEnemyUnit(5,-18,22,23,3), new GeneratedEnemyUnit(6,19,26,48,1), new GeneratedEnemyUnit(19,17,20,23,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "80aefcbbd81397316115aeca5a00ff9abb01a82711a56c4f9195d07cc0a0f9da");
        }

        private static void Case_04279()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4279,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,7,74,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,18,25,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,0,59,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-9,51,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-9,27,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,18,40,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,8,5,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,14,88,23,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "dbfb2fb7149b656e873272ad80d72415d19487ddc03a558c19fd31df692fa139");
        }

        private static void Case_04280()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4280,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-3,32,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-7,88,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-1,93,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-4,33,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-18,43,12,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "787b0ddaf497d789a9f20f045aced15687c9b74245dc8ac6bec6bcb206f68adc");
        }

        private static void Case_04281()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4281,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-8,41,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,1,47,26,4), new GeneratedEnemyUnit(-17,20,90,1,1), new GeneratedEnemyUnit(-19,-8,48,1,1), new GeneratedEnemyUnit(18,9,18,34,3), new GeneratedEnemyUnit(12,-18,44,36,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "0af26f48e2bc7c05235fe13f1066f91082c376637f94cfcf48c1375946a840ba");
        }

        private static void Case_04282()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4282,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,15,5,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,8,31,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-1,53,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-7,88,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,5,27,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-16,67,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-20,56,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-15,8,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,2,97,33,3), new GeneratedEnemyUnit(-13,-18,46,46,3), new GeneratedEnemyUnit(-15,-3,9,31,3), new GeneratedEnemyUnit(1,17,31,11,1), new GeneratedEnemyUnit(8,-20,98,36,1), new GeneratedEnemyUnit(16,-4,69,45,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "e634fb75b2b427f41f18f019122f746f2d5fd99b9861aea913c0c7888aa5220f");
        }

        private static void Case_04283()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4283,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,7,50,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,16,49,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,4,99,30,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "528b12bbe2c6352b720d879a3ea6fe00e99283a0108a858e4f69a1a769b59f44");
        }

        private static void Case_04284()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4284,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,2,72,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,19,21,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,3,12,32,1), new GeneratedEnemyUnit(5,10,50,4,4), new GeneratedEnemyUnit(-10,9,34,45,1), new GeneratedEnemyUnit(16,13,31,30,3), new GeneratedEnemyUnit(17,4,68,23,4), new GeneratedEnemyUnit(-3,11,98,26,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "6ad47f2350e16976d4954b12dcae51169f5ef7bd1a6ad836b0ae4becbff4a04f");
        }

        private static void Case_04285()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4285,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,6,88,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,14,64,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,16,29,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-7,48,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-11,9,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-19,82,28,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "824c3de13713fb52dae5ce598e1284b0ce1e273163acfc04fe3ce9431db3ca09");
        }

        private static void Case_04286()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4286,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-12,91,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-7,71,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,0,28,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,11,70,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-12,51,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-14,55,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-14,29,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,14,42,36,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "212fc488cf8732b53a7b6b56b6306b5c9a6230cb8d2ec5ff34975b99d1fbbceb");
        }

        private static void Case_04287()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4287,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-5,38,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-6,88,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-14,76,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,16,52,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,18,32,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-13,31,7,2), new GeneratedEnemyUnit(3,-4,90,28,1), new GeneratedEnemyUnit(-17,-19,69,38,3), new GeneratedEnemyUnit(-1,13,87,9,3), new GeneratedEnemyUnit(-15,15,92,18,2), new GeneratedEnemyUnit(-2,15,61,48,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "edee58b7a47258b8bb23fb071fa072eeea24ed038b44b0267147cb7d242d24bc");
        }

        private static void Case_04288()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4288,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-15,92,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,6,9,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-5,49,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,8,10,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,19,63,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-14,55,1,2), new GeneratedEnemyUnit(10,1,100,41,2), new GeneratedEnemyUnit(-11,3,10,32,4), new GeneratedEnemyUnit(14,0,70,2,1), new GeneratedEnemyUnit(14,15,40,37,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "4a566d98993a1bd82fb5bbbad5d5b43f14623baf01fba4c34c46a279ca45a5f2");
        }

        private static void Case_04289()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4289,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-1,5,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-16,80,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,11,8,21,2), new GeneratedEnemyUnit(2,-2,12,49,3), new GeneratedEnemyUnit(1,6,66,15,1), new GeneratedEnemyUnit(-9,-8,14,17,2), new GeneratedEnemyUnit(-10,18,6,21,1), new GeneratedEnemyUnit(-11,-18,47,27,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "adcacc295f4fe23b108bba9a9ec77cdf90c8a65a52ed22016b5b2c0ea11e615d");
        }

        private static void Case_04290()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4290,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,6,10,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,5,59,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,4,75,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-15,81,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-4,49,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-4,78,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,13,70,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,15,65,5,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "e4ac6c02392119586a6cd13034d136a02a1ba36ff4ae7150d8391007d04fbca0");
        }

        private static void Case_04291()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4291,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,13,86,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-8,6,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,0,32,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-8,14,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,16,29,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-17,97,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-14,66,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-5,73,11,3), new GeneratedEnemyUnit(0,17,98,21,1), new GeneratedEnemyUnit(-3,-7,98,43,3), new GeneratedEnemyUnit(12,20,87,1,1), new GeneratedEnemyUnit(-9,-20,99,18,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "68be892793f6e6d479fc72014493d08399ac08c8d52e13bb39a158c40c1e6050");
        }

        private static void Case_04292()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4292,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-5,94,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-2,97,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-1,46,48,4), new GeneratedEnemyUnit(-7,-9,81,32,1), new GeneratedEnemyUnit(6,-2,6,16,1), new GeneratedEnemyUnit(6,-3,6,18,4), new GeneratedEnemyUnit(16,0,81,50,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "0a20d6fbb62075258a774dd6cb43a2da71f3ef529477420ee5ff9d828a3d516c");
        }

        private static void Case_04293()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4293,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-3,51,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-12,50,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-10,61,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,20,56,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-6,38,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-15,88,5,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 75,
                stableHash: "b7b3ae14bab99b2c4c6a33a17c20b200950ee724140acb973460e34658060cee");
        }

        private static void Case_04294()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4294,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,17,17,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-7,32,2,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "97555758f9e7caf8c7b8e5b93c3740ec5b6c8706afd067d9b80e45c944d1b423");
        }

        private static void Case_04295()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4295,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,13,55,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-10,65,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-14,63,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,9,91,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,17,73,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,3,24,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,17,23,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,11,76,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,14,33,45,3), new GeneratedEnemyUnit(-15,18,64,10,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "716cb6a54aba9545ed868a4cb9b8697f516a928cd6480a9b43438f7c0ee0d3ab");
        }

        private static void Case_04296()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4296,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-20,20,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-20,83,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,10,10,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,4,34,5,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "b5b0a7ed7855963c28bfd343b06497e6c789297a77420976171bfd16a86fb990");
        }

        private static void Case_04297()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4297,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,10,82,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,7,9,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,9,70,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,15,38,24,2), new GeneratedEnemyUnit(-9,13,93,15,4), new GeneratedEnemyUnit(-2,-15,57,42,1), new GeneratedEnemyUnit(3,-4,48,35,4), new GeneratedEnemyUnit(-15,14,49,37,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "34226487d4ab800312c8fdeaa440ace2e536b3862c141868dbc9a78316b4f77a");
        }

        private static void Case_04298()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4298,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-8,8,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-20,9,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,17,46,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,19,67,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,8,84,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-14,74,28,2), new GeneratedEnemyUnit(0,7,12,1,4), new GeneratedEnemyUnit(-10,14,54,23,3), new GeneratedEnemyUnit(-6,-1,77,28,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "7575f58f2215d928115d771d577342bd795f685d31bfc0b8822ec1950cb164a6");
        }

        private static void Case_04299()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4299,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-9,6,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-9,72,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,17,79,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-6,51,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,7,89,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-18,19,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-5,62,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-6,54,15,3), new GeneratedEnemyUnit(20,18,89,3,2), new GeneratedEnemyUnit(20,-9,36,18,2), new GeneratedEnemyUnit(7,-15,80,27,1), new GeneratedEnemyUnit(-7,-7,30,41,4), new GeneratedEnemyUnit(15,2,51,39,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 55,
                stableHash: "6bac7570bc0861b325e0cb2e49875c3320a45ce3f26c8d15fc812a88f83eec38");
        }

        private static void Case_04300()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4300,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-14,66,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-4,83,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-15,55,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-13,25,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-2,5,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-3,26,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-15,30,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,4,57,2,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,12,16,27,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "c008b99448c3d4bfe3aff2d73f26466e9a9f6f46f4524d73b134667414037aea");
        }

        private static void Case_04301()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4301,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,14,76,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,9,37,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,7,41,46,3), new GeneratedEnemyUnit(-13,11,57,50,2), new GeneratedEnemyUnit(14,-9,73,3,4), new GeneratedEnemyUnit(12,16,84,23,3), new GeneratedEnemyUnit(-4,5,90,20,1), new GeneratedEnemyUnit(-10,-16,81,21,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "7e4eb5d0036a7ac6be2f08a1b8af009cf0b730f7a9d8f261eecc107c3655db8f");
        }

        private static void Case_04302()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4302,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,7,69,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,2,98,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-1,15,7,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "c9bf5e6dfb2c0c06d047068c2ed1bb625413291a6c60998e85d1269ae03e2b0b");
        }

        private static void Case_04303()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4303,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-1,82,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,2,83,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,20,86,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-4,37,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,1,71,15,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "585bd8310bb307443afd1603c9eae6bef962d256c7f4d246948553ac8382d2bb");
        }

        private static void Case_04304()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4304,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-10,51,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,11,81,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,0,14,11,2), new GeneratedEnemyUnit(0,15,93,36,3), new GeneratedEnemyUnit(5,12,7,7,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "3a9b30b12b70eea19781ef877b76b446a2075790bd48b1ff2da5a81cc1521e51");
        }

        private static void Case_04305()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4305,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-4,81,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-11,84,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,8,68,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-20,59,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-12,74,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-11,52,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-15,96,45,2), new GeneratedEnemyUnit(18,-17,61,26,1), new GeneratedEnemyUnit(9,-16,22,26,3), new GeneratedEnemyUnit(2,-5,90,35,3), new GeneratedEnemyUnit(2,-12,53,24,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "0c4471abad4b92c99e0e33b07cd68f1ee9ec51ed3b2c39e300d6907742feaa5e");
        }

        private static void Case_04306()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4306,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,20,20,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,13,56,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-15,78,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-15,81,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,9,50,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,0,70,9,4), new GeneratedEnemyUnit(-4,-12,86,40,2), new GeneratedEnemyUnit(5,4,9,12,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "cb7ef1630af1db4c699fa15e3b8599edb0476f041bbe10863ffc4f619be65de2");
        }

        private static void Case_04307()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4307,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-15,31,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,18,30,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,16,11,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-5,9,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,4,45,19,4), new GeneratedEnemyUnit(-3,-16,41,47,4), new GeneratedEnemyUnit(-6,17,93,48,3), new GeneratedEnemyUnit(13,14,41,11,3), new GeneratedEnemyUnit(9,-7,84,12,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "69d71d471d7bc7fb76391d854a6690ba121e5ad4892571bd13f7bb36872bbc3e");
        }

        private static void Case_04308()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4308,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-7,63,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,12,39,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-19,61,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-5,72,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-18,8,10,2), new GeneratedEnemyUnit(16,4,93,43,1), new GeneratedEnemyUnit(-9,-20,48,6,1), new GeneratedEnemyUnit(0,9,39,35,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "0d745570b8596f4f3a06f9f5bced49b9e5ee701694416a396de948fd40d35da4");
        }

        private static void Case_04309()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4309,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-3,74,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-12,61,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,1,26,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-1,36,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,5,31,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-18,42,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-6,74,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,11,13,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,4,34,5,4), new GeneratedEnemyUnit(-7,3,19,26,1), new GeneratedEnemyUnit(-10,6,33,37,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "72946fda7de1a40d5a3c68e5b14098e2ab8706170c8b160e67ea8492d9b9756f");
        }

        private static void Case_04310()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4310,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-3,29,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,14,66,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-3,81,19,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "dba4e80419c6706264629319b5926270e581edce3723f9bdb560451a4550b360");
        }

        private static void Case_04311()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4311,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,14,38,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,19,75,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-14,24,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,9,41,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-16,77,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-4,12,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-13,94,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-19,72,48,2), new GeneratedEnemyUnit(-16,-18,45,41,3), new GeneratedEnemyUnit(4,-13,23,38,4), new GeneratedEnemyUnit(17,-8,69,8,2), new GeneratedEnemyUnit(2,-16,7,15,4), new GeneratedEnemyUnit(0,13,66,8,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "5676285d98559bf70092404c871cfc1e8d885b5eb24c0cb9e2c3c287e9f703b9");
        }

        private static void Case_04312()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4312,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-10,22,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-8,72,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-15,59,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,15,51,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,16,17,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,19,86,21,2), new GeneratedEnemyUnit(1,-18,72,24,3), new GeneratedEnemyUnit(-2,5,9,24,4), new GeneratedEnemyUnit(-3,-4,48,16,4), new GeneratedEnemyUnit(5,-19,90,49,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "db08c68aaf51502796484a9e3e268b4182ec540a7bc14a2e04b4f618301396b8");
        }

        private static void Case_04313()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4313,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,18,80,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-11,9,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-3,41,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,11,12,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-12,21,3,4), new GeneratedEnemyUnit(10,14,27,36,3), new GeneratedEnemyUnit(-17,5,15,36,2), new GeneratedEnemyUnit(-17,13,89,39,2), new GeneratedEnemyUnit(-8,-7,95,38,1), new GeneratedEnemyUnit(6,-3,33,17,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "b9f5101eef2660627af3ebc8faabdb97235e5549fe634469194143df164a0c37");
        }

        private static void Case_04314()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4314,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-2,40,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,15,93,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,13,97,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-9,81,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-10,99,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,3,54,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,11,25,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,17,82,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-12,69,11,2), new GeneratedEnemyUnit(13,-16,52,10,3), new GeneratedEnemyUnit(5,8,38,38,2), new GeneratedEnemyUnit(-13,-15,96,6,4), new GeneratedEnemyUnit(-10,17,64,33,1), new GeneratedEnemyUnit(13,-18,23,24,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "7e7d815de6a51e8d567db17f7540819e59cc882a75bd3ef220893dfe8ad2ffb6");
        }

        private static void Case_04315()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4315,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-4,80,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,19,66,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,8,100,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-19,89,48,4), new GeneratedEnemyUnit(9,0,70,13,1), new GeneratedEnemyUnit(2,-1,49,36,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "07e941c82073f978957a2eca674729df6c399e0a49804a46543539e1cc2ab6ac");
        }

        private static void Case_04316()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4316,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,20,68,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,16,93,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,5,11,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-19,66,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,13,32,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-15,79,8,1), new GeneratedEnemyUnit(-11,6,44,4,1), new GeneratedEnemyUnit(13,-7,95,28,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "7a565096f694ab4aa931310a354094d35309ad8cef7c463de2621d44f12fbccd");
        }

        private static void Case_04317()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4317,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,12,47,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,9,58,33,2), new GeneratedEnemyUnit(-4,20,10,21,4), new GeneratedEnemyUnit(-9,-7,91,34,3), new GeneratedEnemyUnit(-12,11,44,9,1), new GeneratedEnemyUnit(-12,14,80,43,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "6ba26fa38f9c9d616783a611ce7a1bf57caf560f733b6ea3f7a21907e9dc83fa");
        }

        private static void Case_04318()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4318,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-18,40,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-2,17,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,3,20,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-1,67,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,2,45,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,11,17,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,6,98,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,5,53,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,12,52,6,2), new GeneratedEnemyUnit(16,-12,91,9,3), new GeneratedEnemyUnit(4,14,87,8,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "aaf06ba6fcf4ab4456651bf06222f1700a6610e2838cdfc667cbfb2be4090bde");
        }

        private static void Case_04319()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4319,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-12,7,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-5,42,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-9,42,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-9,49,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-14,83,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-3,90,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-3,92,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,10,54,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,15,32,18,3), new GeneratedEnemyUnit(18,-19,45,36,4), new GeneratedEnemyUnit(18,-7,25,22,3), new GeneratedEnemyUnit(15,15,72,10,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "f150fac7571fa75d120fb9a7bbed40f3226c6851b0dc1a2c3e527bb20fdb19a1");
        }

    }
}
