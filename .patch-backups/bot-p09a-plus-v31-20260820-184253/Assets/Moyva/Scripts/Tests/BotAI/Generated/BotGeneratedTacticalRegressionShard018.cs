using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard018
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_03240();
            Case_03241();
            Case_03242();
            Case_03243();
            Case_03244();
            Case_03245();
            Case_03246();
            Case_03247();
            Case_03248();
            Case_03249();
            Case_03250();
            Case_03251();
            Case_03252();
            Case_03253();
            Case_03254();
            Case_03255();
            Case_03256();
            Case_03257();
            Case_03258();
            Case_03259();
            Case_03260();
            Case_03261();
            Case_03262();
            Case_03263();
            Case_03264();
            Case_03265();
            Case_03266();
            Case_03267();
            Case_03268();
            Case_03269();
            Case_03270();
            Case_03271();
            Case_03272();
            Case_03273();
            Case_03274();
            Case_03275();
            Case_03276();
            Case_03277();
            Case_03278();
            Case_03279();
            Case_03280();
            Case_03281();
            Case_03282();
            Case_03283();
            Case_03284();
            Case_03285();
            Case_03286();
            Case_03287();
            Case_03288();
            Case_03289();
            Case_03290();
            Case_03291();
            Case_03292();
            Case_03293();
            Case_03294();
            Case_03295();
            Case_03296();
            Case_03297();
            Case_03298();
            Case_03299();
            Case_03300();
            Case_03301();
            Case_03302();
            Case_03303();
            Case_03304();
            Case_03305();
            Case_03306();
            Case_03307();
            Case_03308();
            Case_03309();
            Case_03310();
            Case_03311();
            Case_03312();
            Case_03313();
            Case_03314();
            Case_03315();
            Case_03316();
            Case_03317();
            Case_03318();
            Case_03319();
            Case_03320();
            Case_03321();
            Case_03322();
            Case_03323();
            Case_03324();
            Case_03325();
            Case_03326();
            Case_03327();
            Case_03328();
            Case_03329();
            Case_03330();
            Case_03331();
            Case_03332();
            Case_03333();
            Case_03334();
            Case_03335();
            Case_03336();
            Case_03337();
            Case_03338();
            Case_03339();
            Case_03340();
            Case_03341();
            Case_03342();
            Case_03343();
            Case_03344();
            Case_03345();
            Case_03346();
            Case_03347();
            Case_03348();
            Case_03349();
            Case_03350();
            Case_03351();
            Case_03352();
            Case_03353();
            Case_03354();
            Case_03355();
            Case_03356();
            Case_03357();
            Case_03358();
            Case_03359();
            Case_03360();
            Case_03361();
            Case_03362();
            Case_03363();
            Case_03364();
            Case_03365();
            Case_03366();
            Case_03367();
            Case_03368();
            Case_03369();
            Case_03370();
            Case_03371();
            Case_03372();
            Case_03373();
            Case_03374();
            Case_03375();
            Case_03376();
            Case_03377();
            Case_03378();
            Case_03379();
            Case_03380();
            Case_03381();
            Case_03382();
            Case_03383();
            Case_03384();
            Case_03385();
            Case_03386();
            Case_03387();
            Case_03388();
            Case_03389();
            Case_03390();
            Case_03391();
            Case_03392();
            Case_03393();
            Case_03394();
            Case_03395();
            Case_03396();
            Case_03397();
            Case_03398();
            Case_03399();
            Case_03400();
            Case_03401();
            Case_03402();
            Case_03403();
            Case_03404();
            Case_03405();
            Case_03406();
            Case_03407();
            Case_03408();
            Case_03409();
            Case_03410();
            Case_03411();
            Case_03412();
            Case_03413();
            Case_03414();
            Case_03415();
            Case_03416();
            Case_03417();
            Case_03418();
            Case_03419();
        }

        private static void Case_03240()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3240,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-13,97,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,16,71,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,5,78,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-13,89,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,13,62,13,3), new GeneratedEnemyUnit(-19,7,77,49,3), new GeneratedEnemyUnit(-15,-17,94,14,3), new GeneratedEnemyUnit(-3,-5,25,37,4), new GeneratedEnemyUnit(19,5,54,17,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "32f0bf02eb0475ad97aa710fcd1c9d2863c5b5401e9c06e17e90fd69ef854212");
        }

        private static void Case_03241()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3241,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-6,28,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,9,53,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,19,47,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-3,65,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-8,20,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-16,82,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,17,20,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-8,58,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-3,19,25,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "fc96efcade6ff892bfd71c7f3057189219ebe66ff6af2a4d9e7a6716f2ea383d");
        }

        private static void Case_03242()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3242,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-7,14,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-11,65,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-13,45,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-9,15,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,19,11,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,19,74,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-20,10,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,1,86,14,1), new GeneratedEnemyUnit(-1,20,56,20,1), new GeneratedEnemyUnit(-12,1,44,43,1), new GeneratedEnemyUnit(-9,-13,21,35,2), new GeneratedEnemyUnit(18,8,27,28,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "29949628bebf89f8c7a14894569711bde5131e4a9fc8d324f3c28354f1d5d35d");
        }

        private static void Case_03243()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3243,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,7,13,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,8,12,29,1), new GeneratedEnemyUnit(-14,-8,45,47,3), new GeneratedEnemyUnit(7,-9,90,35,3), new GeneratedEnemyUnit(-1,-20,83,29,3), new GeneratedEnemyUnit(-20,6,81,10,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "59a5ee2dd56f6ff0ddff76c52868841dc13f7bf2789aa04f4e35d0b98180ad0d");
        }

        private static void Case_03244()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3244,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-8,12,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,5,52,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-8,98,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,8,75,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-1,67,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-15,46,28,3), new GeneratedEnemyUnit(-6,15,31,35,3), new GeneratedEnemyUnit(-7,-2,95,27,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "8c9b5028d46bfa1b7a7b38b9c0cdb143bab82135a8187f1708aad7967496979e");
        }

        private static void Case_03245()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3245,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,12,54,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,5,23,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,15,31,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,5,79,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-5,90,8,1), new GeneratedEnemyUnit(-15,-7,7,34,2), new GeneratedEnemyUnit(12,-10,48,7,2), new GeneratedEnemyUnit(4,9,82,43,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "7a0f79b501f66d0545f6783a4b5edcb3d67ac492df72817dc33964e8331d17ea");
        }

        private static void Case_03246()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3246,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-17,51,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-11,54,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,8,43,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,10,67,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-10,37,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,6,67,10,2), new GeneratedEnemyUnit(-9,-17,57,35,2), new GeneratedEnemyUnit(16,20,90,48,2), new GeneratedEnemyUnit(-8,20,78,19,2), new GeneratedEnemyUnit(-13,9,64,49,1), new GeneratedEnemyUnit(18,20,53,22,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 55,
                stableHash: "95ef37747644271e901ffaec418c625d3b3dcf3bd66e7966ad78155f0034379a");
        }

        private static void Case_03247()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3247,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,8,64,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,5,92,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-2,43,35,3), new GeneratedEnemyUnit(-13,1,82,33,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "8a6b868dd8434c444a52575edbbecfdd574b4d326d80034a9dbf09a3232b639e");
        }

        private static void Case_03248()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3248,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,15,87,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-9,59,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-2,61,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-13,47,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,13,11,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,11,76,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-1,36,47,1), new GeneratedEnemyUnit(-2,19,40,43,3), new GeneratedEnemyUnit(7,-1,66,8,1), new GeneratedEnemyUnit(4,-20,89,49,2), new GeneratedEnemyUnit(-16,0,7,46,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "92a308d451dc54831b6e5127e79660121aa182cec99282f37ef9b731c0de356e");
        }

        private static void Case_03249()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3249,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-2,80,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,12,25,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-9,43,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-5,96,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,13,75,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,8,95,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,15,97,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-13,71,25,2), new GeneratedEnemyUnit(-7,-15,74,9,3), new GeneratedEnemyUnit(8,-14,68,10,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "e0c2078c84858585c48658c8c70839644c3678439f0a3ff5793ed227cc143263");
        }

        private static void Case_03250()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3250,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,16,45,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-20,15,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-8,25,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-18,68,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,2,72,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-12,97,26,4), new GeneratedEnemyUnit(-8,-4,12,40,4), new GeneratedEnemyUnit(15,-18,5,8,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "7fa6eed25938ac31a3e105b7c52173d57bb20c10f2ae181bde91dc8e57fcbb15");
        }

        private static void Case_03251()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3251,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-20,72,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-18,50,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-19,37,6,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "88be155e3af402e38a74f28abfb63e471342320cebaecad484127675dfce9201");
        }

        private static void Case_03252()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3252,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-19,76,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-11,22,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,6,16,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-18,88,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,9,46,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,6,99,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,15,59,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,14,53,19,4), new GeneratedEnemyUnit(2,-17,73,31,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "20cc11f5360aaebe498a0f64d94f468e3fd94383a6183b100c1b06c0ba4fa92d");
        }

        private static void Case_03253()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3253,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,3,44,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-7,89,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-8,28,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,2,46,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,6,24,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-16,36,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-4,99,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,15,32,29,4), new GeneratedEnemyUnit(-12,13,49,36,1), new GeneratedEnemyUnit(2,10,40,47,2), new GeneratedEnemyUnit(-17,4,8,9,1), new GeneratedEnemyUnit(16,-1,28,16,2), new GeneratedEnemyUnit(11,-17,8,9,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "912080a9ded7ab6d5f2cf06a30b1c121f3973749a2a2cd4a085085c7f65878ae");
        }

        private static void Case_03254()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3254,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-14,82,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,18,81,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-14,83,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-5,59,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,15,82,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,0,66,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,16,50,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,6,88,15,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "f6b94f42f33deaaade602f3b7d3675bfb4a9a44de491d820626fa3b92108413a");
        }

        private static void Case_03255()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3255,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,0,74,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-1,54,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,17,6,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-2,63,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-6,72,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-10,99,25,2), new GeneratedEnemyUnit(8,7,63,27,4), new GeneratedEnemyUnit(-5,3,21,12,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "d217cfe7fddace0fe6cce36c8d799220f2994d6050c35c46b22246e221dca9ba");
        }

        private static void Case_03256()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3256,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,11,46,7,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "6031a09689680e7b62339456a266ac80d4082fcb4b0d329bb51a07ace465312c");
        }

        private static void Case_03257()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3257,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-12,8,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-4,52,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,14,16,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-1,78,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,16,32,3,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "fc5c43157ed4917ae0df4a23b2870edfb3f54042600524caa7ff697cca054cf0");
        }

        private static void Case_03258()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3258,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,2,89,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,4,68,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,13,35,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,6,80,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,18,8,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-11,34,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-3,58,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,3,79,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,12,43,19,3), new GeneratedEnemyUnit(-7,11,5,15,4), new GeneratedEnemyUnit(14,-9,15,33,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "ef80cc80543577386c5acdcf672d1585597c4b781b51c16c06f9aff3b1a7d0c5");
        }

        private static void Case_03259()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3259,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-17,51,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-6,41,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,11,32,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,7,17,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-20,47,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,10,53,44,1), new GeneratedEnemyUnit(9,-10,71,20,2), new GeneratedEnemyUnit(-6,-3,48,32,2), new GeneratedEnemyUnit(-16,1,52,5,2), new GeneratedEnemyUnit(-10,-3,32,9,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "a1b3c1d1ce013e0af3a6982d684890b17b51589bc2a10593fc41375ca8607d67");
        }

        private static void Case_03260()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3260,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-1,69,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,15,20,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-11,75,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,16,56,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-8,10,47,2), new GeneratedEnemyUnit(17,5,48,44,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "bd7407b0422de911aee8843f364f8bb95a976f9843d5aa8ae32c457b4db25751");
        }

        private static void Case_03261()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3261,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-20,75,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-10,99,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,15,14,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,1,10,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-13,93,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,19,19,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,15,58,19,1), new GeneratedEnemyUnit(-8,16,98,7,2), new GeneratedEnemyUnit(-11,0,40,47,1), new GeneratedEnemyUnit(9,4,42,25,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "e2ca4397f9bbabe14ecf2ff03034e697ac775e46d62571213be05855dfb51283");
        }

        private static void Case_03262()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3262,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-3,67,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,18,21,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,18,84,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,10,66,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,16,51,49,1), new GeneratedEnemyUnit(0,-20,90,32,4), new GeneratedEnemyUnit(10,10,28,29,4), new GeneratedEnemyUnit(-2,-8,66,5,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "414a94829c75f7b75937e112b06d23ecde15ba8689f751cc181adb4b4c92a88e");
        }

        private static void Case_03263()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3263,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,0,81,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,16,79,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-1,62,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,14,92,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-18,48,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,0,60,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,19,85,22,3), new GeneratedEnemyUnit(8,-7,98,50,3), new GeneratedEnemyUnit(14,1,21,44,3), new GeneratedEnemyUnit(10,20,45,1,2), new GeneratedEnemyUnit(13,-19,81,6,3), new GeneratedEnemyUnit(-12,1,80,14,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "f644a72f4bf141a242f193a5f18e49e1eea727cb0bd7075681e8fffc773186bd");
        }

        private static void Case_03264()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3264,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-3,43,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-11,68,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,19,19,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,12,44,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-7,74,25,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "9836ea2dc71681c37441d54b34259268ab212de8687a5ce0d1e75fdbfff82817");
        }

        private static void Case_03265()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3265,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,15,25,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,19,33,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,18,78,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,16,9,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-1,10,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,8,43,49,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "90e5c8cfe8916c9c037085a6c1197b042df1f6baa576a9282cec8ece3d51cbac");
        }

        private static void Case_03266()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3266,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,15,35,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,8,27,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,0,29,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-7,72,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,0,52,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,8,48,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,20,65,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,1,26,13,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "f2464d05003fe0ea129b7233a46eb6c0d437e1866dc97288717f75f9f67f0d7b");
        }

        private static void Case_03267()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3267,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-17,68,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,3,41,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-3,53,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,9,99,46,1), new GeneratedEnemyUnit(-7,14,21,43,1), new GeneratedEnemyUnit(-15,3,82,22,2), new GeneratedEnemyUnit(10,18,13,35,1), new GeneratedEnemyUnit(2,14,51,12,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "b8c86b8f60d50f98f0ee3cab1f45eb46bb72123a8d3705674ee51dd878e2cce3");
        }

        private static void Case_03268()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3268,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,3,35,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-20,91,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-12,56,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-16,16,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-9,50,2,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-4,19,30,4), new GeneratedEnemyUnit(-6,12,29,49,4), new GeneratedEnemyUnit(10,-6,53,24,3), new GeneratedEnemyUnit(-10,-8,14,29,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "66507186aa1b352c8f264a39cc6cb1af0455d8adaef4c9f99d2fe1ab4daf4d20");
        }

        private static void Case_03269()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3269,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-15,30,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,10,22,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,7,74,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,15,20,3,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "68a92c0efd1248a9e5158790512d4ac56451972da7732c8b71e7351ee7be1560");
        }

        private static void Case_03270()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3270,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,3,33,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,10,86,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-4,43,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-2,75,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,16,43,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,17,72,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-5,82,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,18,99,7,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-18,46,34,2), new GeneratedEnemyUnit(19,8,51,13,3), new GeneratedEnemyUnit(-11,15,39,44,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "f4ee9a70370c520398cfff065a06d3a88df0bf41eeedf68ea87f4fa5470b1b4c");
        }

        private static void Case_03271()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3271,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-20,15,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,18,81,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,0,45,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,14,100,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-11,13,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-20,30,14,3), new GeneratedEnemyUnit(7,17,34,48,2), new GeneratedEnemyUnit(10,-11,24,40,1), new GeneratedEnemyUnit(-14,-3,56,3,3), new GeneratedEnemyUnit(19,-3,5,5,4), new GeneratedEnemyUnit(-13,13,48,48,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "0f7d3daf982a8a7d2f0936b6e80a8cbf3dfde38f0bb8072c128f67e2dd1990e5");
        }

        private static void Case_03272()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3272,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,6,93,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-5,31,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-20,54,43,3), new GeneratedEnemyUnit(18,5,73,6,2), new GeneratedEnemyUnit(-16,1,50,45,3), new GeneratedEnemyUnit(16,0,72,3,1), new GeneratedEnemyUnit(9,-18,11,5,4), new GeneratedEnemyUnit(-20,-2,34,31,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "77a934fa14b778e5636f023e2fc62331bbe3a24fd93ff30b3a9b271b74b872a6");
        }

        private static void Case_03273()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3273,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-15,59,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-7,77,2,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-16,79,11,1), new GeneratedEnemyUnit(4,-3,70,34,3), new GeneratedEnemyUnit(5,-8,71,10,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "83ecb2d6ea0e534d88837d636c1b3dcc60af7acaaad6d086f9c34d75cf74cadf");
        }

        private static void Case_03274()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3274,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-9,93,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-8,37,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,0,44,44,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "4b3798107e891b2b4895820c25743ffd1658a418e5b39dadc0ffdd79782f26b8");
        }

        private static void Case_03275()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3275,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,15,26,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,9,23,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-4,14,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,19,83,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-3,78,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,0,57,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,9,56,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,9,88,28,1), new GeneratedEnemyUnit(20,-3,69,1,2), new GeneratedEnemyUnit(-15,5,64,22,3), new GeneratedEnemyUnit(2,-1,20,39,3), new GeneratedEnemyUnit(2,3,58,12,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "5778366a08bda6b77f1e60d58f09dff11fb5a214d0dcae9868d7a3e85e3ecf97");
        }

        private static void Case_03276()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3276,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,9,16,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,6,66,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,13,91,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,15,30,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,7,40,1,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "7916bdaa8406d290523934689d4dccd271e25b05c50689c2dab53cd3271e34a7");
        }

        private static void Case_03277()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3277,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,11,29,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-11,96,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-19,83,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,20,91,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-16,18,38,1), new GeneratedEnemyUnit(-17,9,53,7,2), new GeneratedEnemyUnit(8,7,60,2,4), new GeneratedEnemyUnit(-15,-18,13,38,3), new GeneratedEnemyUnit(-18,-17,18,19,4), new GeneratedEnemyUnit(3,-7,57,26,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "9d0ae1fc8b5bcfb8c929893e91f510c443b5894a9fa8bfbd2bfcef93519b37e1");
        }

        private static void Case_03278()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3278,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-16,22,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-9,58,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-14,74,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,17,50,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,19,88,2,1), new GeneratedEnemyUnit(16,-6,48,47,2), new GeneratedEnemyUnit(14,4,82,24,2), new GeneratedEnemyUnit(1,10,34,36,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "6c4fac6b9b8036ef1597ec2d9019416045e4020abc24c9596f0b8937cadda283");
        }

        private static void Case_03279()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3279,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-10,75,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,5,10,12,2), new GeneratedEnemyUnit(8,-18,77,6,3), new GeneratedEnemyUnit(11,-20,39,44,4), new GeneratedEnemyUnit(-14,-1,65,45,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "dc4d95b3687050ae1d49d7202e6a42b37385faa1ba298c8c45407244c3e9ac84");
        }

        private static void Case_03280()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3280,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-5,13,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,4,29,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-8,26,8,1), new GeneratedEnemyUnit(4,-4,40,39,3), new GeneratedEnemyUnit(-3,13,98,48,1), new GeneratedEnemyUnit(-7,14,9,2,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "b93359384bef1023a585cfd22ff11cb63399a994a3e58b955ff60909ccf68ac5");
        }

        private static void Case_03281()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3281,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-1,43,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,14,54,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-18,53,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,7,65,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-12,96,28,1), new GeneratedEnemyUnit(10,-1,86,46,4), new GeneratedEnemyUnit(5,-2,19,34,1), new GeneratedEnemyUnit(14,-20,84,44,1), new GeneratedEnemyUnit(9,4,91,31,4), new GeneratedEnemyUnit(6,-2,69,46,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "cfc197d8b10d40ed5ba43240efa370266536e2309b878ba616695e30797e1424");
        }

        private static void Case_03282()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3282,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-8,60,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-8,51,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-13,45,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,14,70,16,2), new GeneratedEnemyUnit(-15,-11,19,36,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "7d64ecc66ddaa0c5d35deebd4d2731efc633c1aadb766dffeaa58adf935d4e85");
        }

        private static void Case_03283()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3283,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,5,77,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-2,99,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-7,6,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-12,83,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-4,49,40,1), new GeneratedEnemyUnit(16,-13,73,26,4), new GeneratedEnemyUnit(14,-9,13,34,1), new GeneratedEnemyUnit(14,11,39,15,4), new GeneratedEnemyUnit(4,-4,69,31,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "107310438fe5a8e8dd7f08d2c41d7ca61af5771ae74dddcdbc984e2bc306e901");
        }

        private static void Case_03284()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3284,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-5,60,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,12,70,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,3,95,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-2,77,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-12,39,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-12,68,4,3), new GeneratedEnemyUnit(-3,1,24,25,1), new GeneratedEnemyUnit(-9,-7,91,25,3), new GeneratedEnemyUnit(2,-12,21,35,3), new GeneratedEnemyUnit(11,12,54,46,3), new GeneratedEnemyUnit(19,-15,80,15,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "c0ef07ba3f48e960ae25bfdf1803ede9b8a69276612db3c485cb5bae6fea3224");
        }

        private static void Case_03285()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3285,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,2,8,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-8,58,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-12,51,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,7,77,20,1), new GeneratedEnemyUnit(11,11,7,7,2), new GeneratedEnemyUnit(14,0,28,25,2), new GeneratedEnemyUnit(19,13,17,14,1), new GeneratedEnemyUnit(16,-9,34,14,4), new GeneratedEnemyUnit(-17,-9,82,30,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "ed9bf9f996299106f1136e5ab6e9a7dc41486b554ce2c8de958f15ea3d6aa932");
        }

        private static void Case_03286()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3286,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,6,21,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,10,20,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,16,83,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,14,21,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,3,79,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,8,42,18,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "6203523ab0c7184a87d093920f18540d6a8d66a16c270221e830027f4fa2e7e4");
        }

        private static void Case_03287()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3287,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,7,79,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-15,17,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,3,18,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-18,63,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-9,82,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,11,8,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-13,64,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-19,96,2,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "271853ceefed86b44d10f65f83dc78d3d36a6155fe4be02003a1e8aa5b379a22");
        }

        private static void Case_03288()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3288,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,6,48,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-12,79,30,4), new GeneratedEnemyUnit(8,-13,91,25,4), new GeneratedEnemyUnit(17,11,69,26,3), new GeneratedEnemyUnit(-3,16,19,1,4), new GeneratedEnemyUnit(-16,4,100,17,1), new GeneratedEnemyUnit(17,7,63,15,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "8ae8d8f609b2c825df1b00f64a941492529a3762361c5090d9412d173530fa9f");
        }

        private static void Case_03289()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3289,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,2,34,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,17,18,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,2,94,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,15,59,12,3), new GeneratedEnemyUnit(-6,14,55,16,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "0edafd5c7ca657bbf4627b64db1667e3fe6f0e4a05aa86af689d62cc7b4e6969");
        }

        private static void Case_03290()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3290,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,3,37,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-6,15,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,18,36,26,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "804d9f226f2d6f68878c82d54e24c862e057c526db3253b6940ca49b4820fec2");
        }

        private static void Case_03291()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3291,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,17,92,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,10,77,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,12,91,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-3,34,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-6,33,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-4,55,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-4,43,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,20,35,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-10,58,18,4), new GeneratedEnemyUnit(-17,-18,77,29,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "8ea17c13c3a78162ae724587862204dce71efb88c48c16eae1996533ac8b1fb8");
        }

        private static void Case_03292()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3292,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-15,64,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,3,42,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,8,39,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-13,31,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,4,50,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-3,5,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-8,22,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,18,24,45,2), new GeneratedEnemyUnit(9,-20,48,20,4), new GeneratedEnemyUnit(6,0,74,14,2), new GeneratedEnemyUnit(11,16,71,14,2), new GeneratedEnemyUnit(12,-3,74,44,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "9e9c52885937571fcae9fe0ef97829fd2e28594defd311062cfbbb080b191736");
        }

        private static void Case_03293()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3293,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,5,74,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-20,38,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,11,24,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-18,74,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,6,89,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,1,11,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-7,12,50,2), new GeneratedEnemyUnit(7,17,83,6,1), new GeneratedEnemyUnit(14,-7,25,9,1), new GeneratedEnemyUnit(-14,2,26,5,3), new GeneratedEnemyUnit(5,-11,16,47,2), new GeneratedEnemyUnit(-10,-7,78,45,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "4d3d44bce309cfd8cb17fadf3fabf898c76b2464fbf247a5647da495150efa31");
        }

        private static void Case_03294()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3294,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-17,43,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,3,100,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,19,78,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,11,57,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,0,10,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-17,94,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,20,43,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,2,59,19,4), new GeneratedEnemyUnit(0,19,11,40,2), new GeneratedEnemyUnit(-18,19,85,9,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "635b21cfc5fdc2fcd4b1ab5970cb8420d5c304cdc4d708ee12fd83cf65bde95b");
        }

        private static void Case_03295()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3295,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-19,30,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,1,19,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-19,30,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,19,55,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,14,35,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-5,47,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-15,7,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-17,9,17,1), new GeneratedEnemyUnit(13,2,23,7,3), new GeneratedEnemyUnit(-20,20,63,44,1), new GeneratedEnemyUnit(3,-12,45,31,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "148f31279280e7e7a070e9f20e557461b60e5940fc01400b52bd3fad2bf6a28d");
        }

        private static void Case_03296()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3296,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-20,98,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,7,30,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,2,27,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,8,17,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-10,42,29,4), new GeneratedEnemyUnit(20,13,96,8,1), new GeneratedEnemyUnit(13,-2,19,18,3), new GeneratedEnemyUnit(7,12,88,3,2), new GeneratedEnemyUnit(-9,5,27,48,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "754215aaff914ee9036ac34f2917459627c778eef8d4b8ac9adcb60008db25ba");
        }

        private static void Case_03297()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3297,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,1,83,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,5,76,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,0,72,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-18,35,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-8,50,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-14,68,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,4,76,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,20,5,11,3), new GeneratedEnemyUnit(-1,10,12,12,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "90bd8b3d4f9ecf2e4d782e8ae410cd787dbd9b52e1f01e3253f4921ba002b3cf");
        }

        private static void Case_03298()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3298,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-4,94,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-4,95,1,4), new GeneratedEnemyUnit(17,-11,64,50,1), new GeneratedEnemyUnit(12,-2,75,24,1), new GeneratedEnemyUnit(-6,7,6,14,4), new GeneratedEnemyUnit(-1,-20,60,26,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "e2b2014cbec48e47034b67cc6508763d52fe5c614aaef3a14e009ca6a60b368a");
        }

        private static void Case_03299()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3299,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,6,27,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,0,43,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,2,89,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-13,14,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,3,97,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,13,53,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-9,75,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,10,63,10,1), new GeneratedEnemyUnit(15,-9,9,2,2), new GeneratedEnemyUnit(16,17,91,19,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "f00031efd194575d6259b832d45a005f48169c0c817a9333570c4adae9be864e");
        }

        private static void Case_03300()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3300,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,20,94,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,8,15,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-16,65,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,9,45,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,19,70,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,0,34,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-7,93,30,3), new GeneratedEnemyUnit(2,18,20,20,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "6c3fda95f2e9a75d9969f2693094bb939e57e919e5bfb22830bface61668c74c");
        }

        private static void Case_03301()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3301,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,1,11,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-2,7,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-19,61,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,2,35,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,0,72,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-7,51,8,1), new GeneratedEnemyUnit(16,-7,90,40,1), new GeneratedEnemyUnit(16,-16,85,5,3), new GeneratedEnemyUnit(6,-8,85,8,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "ca30350e414c3a16de197f7ea55e4871b53695a0d5d0b78cb61bd58b12f1d92b");
        }

        private static void Case_03302()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3302,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,15,39,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-7,33,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,15,34,2,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "0d6821b812c2dd2d76a4595c98f97680954ede39c7c24708c53e74a262d62d85");
        }

        private static void Case_03303()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3303,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-15,49,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-4,27,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,8,16,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,19,35,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-10,93,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,13,30,45,1), new GeneratedEnemyUnit(-18,17,99,39,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "ae72d7ca90558fd10f34dfe57e28c2d5dc9935185e8ef1a11ab93ad1bf2a8da6");
        }

        private static void Case_03304()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3304,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-5,57,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,20,77,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,12,76,37,2), new GeneratedEnemyUnit(-10,-5,30,27,2), new GeneratedEnemyUnit(14,-13,91,42,2), new GeneratedEnemyUnit(7,15,19,50,4), new GeneratedEnemyUnit(6,0,22,17,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "df4e982597ba990fe306811289959c73e1b99f7b64a4703153033ac22cca478d");
        }

        private static void Case_03305()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3305,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,19,33,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,15,11,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,19,87,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,4,85,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-7,85,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-11,40,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,9,74,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-6,19,44,4), new GeneratedEnemyUnit(-15,-17,32,33,1), new GeneratedEnemyUnit(-13,-20,31,40,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "c1e20bee44dc0f2e55269f29d3828d9b45d23aad9cf74bf423cc16fcc1e6e156");
        }

        private static void Case_03306()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3306,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,20,12,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,14,56,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,17,56,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-6,98,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,11,63,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,6,28,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,20,16,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-9,30,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,9,54,34,2), new GeneratedEnemyUnit(-8,19,31,1,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "1d6ba82ecba183c5ac87d2bcd098d818b60fa9881a5f341873bdd8269778c2af");
        }

        private static void Case_03307()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3307,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,12,30,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-2,62,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-9,76,38,1), new GeneratedEnemyUnit(8,-16,71,1,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "96f237346d975bb1baf9baef9e00dbfe00c358c25ce3bb2fd6bf5d9af93249aa");
        }

        private static void Case_03308()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3308,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,5,78,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-16,17,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,18,55,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,8,92,31,1), new GeneratedEnemyUnit(5,-17,29,1,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "d00e793fe99b53f8e560430bc1a1006831bf72613dc5bdaf6269e58c38fd5632");
        }

        private static void Case_03309()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3309,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,1,82,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-7,90,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-7,45,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-10,42,14,3), new GeneratedEnemyUnit(-5,19,60,47,4), new GeneratedEnemyUnit(-14,8,69,6,3), new GeneratedEnemyUnit(10,-1,72,2,4), new GeneratedEnemyUnit(18,-4,96,24,2), new GeneratedEnemyUnit(3,-11,39,16,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "ec8e42f0db543b47966eda0b97c9573a44ed1b8dea7c2318a7ab6cefbd148879");
        }

        private static void Case_03310()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3310,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-20,32,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-2,84,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-6,40,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,17,57,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-8,64,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-16,51,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,16,15,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,14,16,1,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "00d3926ec1308ed272ed5b047bfb5b52c36dbe165de3654d4903ad42367f7a90");
        }

        private static void Case_03311()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3311,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,0,25,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-4,37,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-4,24,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,20,66,46,2), new GeneratedEnemyUnit(-17,5,60,45,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "237815b7d96199154f9ac9334378884623fe5716bb357d8f927ef6e9762cc96b");
        }

        private static void Case_03312()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3312,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-2,21,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,19,9,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-11,68,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-10,91,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,17,29,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-14,24,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-8,66,50,4), new GeneratedEnemyUnit(-13,-15,90,18,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "d0637ac78c0cd44ffa914bd07aae804122784028b947de191c8597fd4a989e49");
        }

        private static void Case_03313()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3313,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,11,81,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-6,7,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-1,34,34,3), new GeneratedEnemyUnit(10,14,39,32,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "8ab9d4d4b8ade78421f1b9a00d0d047a5bd14524c79c4dbed125f4a5952654fb");
        }

        private static void Case_03314()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3314,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,11,53,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,12,82,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,5,97,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,13,75,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-6,38,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,13,82,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-10,6,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-15,76,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,10,85,3,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "f39b3a0d409cad43c449aa1e37a5fbba9af4691433c3fd20a94cbf158a2033c0");
        }

        private static void Case_03315()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3315,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,7,67,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,17,37,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,11,24,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,8,84,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,15,91,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,1,83,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,9,8,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-15,35,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-11,48,39,2), new GeneratedEnemyUnit(-5,-1,84,47,3), new GeneratedEnemyUnit(-11,-10,28,30,1), new GeneratedEnemyUnit(-3,-15,99,47,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "6ca8a3e9003cdbf3f6c6b70bf1cbc40213d602f13227acd4a8ee96fbb8d05b02");
        }

        private static void Case_03316()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3316,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-16,55,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,17,9,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,7,80,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-5,97,26,4), new GeneratedEnemyUnit(2,-1,55,41,3), new GeneratedEnemyUnit(-20,7,11,6,3), new GeneratedEnemyUnit(-19,19,64,48,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "fd88f9e65dcee534821d3d071a1207c416977c1ed2ed46647084145540ad6eb1");
        }

        private static void Case_03317()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3317,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,0,37,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,9,88,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-6,56,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-3,32,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-6,55,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,17,63,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-12,29,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-4,59,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-20,32,4,1), new GeneratedEnemyUnit(19,2,37,47,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 80,
                stableHash: "0dd17a871a0ece5d3fcdf8760e70dfd94f5d41fa9070f5fa0b720a3d5ed666c0");
        }

        private static void Case_03318()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3318,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-11,66,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-20,36,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,8,24,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-4,60,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,7,12,17,1), new GeneratedEnemyUnit(-8,-5,82,15,3), new GeneratedEnemyUnit(4,5,56,30,1), new GeneratedEnemyUnit(-3,-20,23,31,1), new GeneratedEnemyUnit(-12,-4,18,14,2), new GeneratedEnemyUnit(15,20,21,41,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 45,
                stableHash: "942891b5a8e1b622c895cdc4ce330061d59e2b3bdfd0ca6a243b4935d39de3a9");
        }

        private static void Case_03319()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3319,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,11,39,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-11,71,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,20,91,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,1,61,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,8,82,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-1,92,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-4,31,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,20,76,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,18,83,38,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "0c10b6d577e884c001f8bf23222f3aea884bd895ae4da0f517339f9582ea0090");
        }

        private static void Case_03320()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3320,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-6,69,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,1,59,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,16,30,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,5,37,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,15,100,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-7,27,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,20,31,37,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "6e5d57f5e302526b0bf2c4c369ac17fd324e9ba1e0e8c72590ba9dfcb27ad7da");
        }

        private static void Case_03321()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3321,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-8,68,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,4,31,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,6,39,6,4), new GeneratedEnemyUnit(-15,17,15,5,2), new GeneratedEnemyUnit(-1,-16,38,15,3), new GeneratedEnemyUnit(-14,-14,38,8,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "2842064047d4832cd9d46dfade326d00ce7fbd74b4369de47ca4b268267e49b2");
        }

        private static void Case_03322()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3322,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-7,34,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-4,48,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,4,92,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,2,6,4,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "e5287095fcd5e794d46c64b21f1e1f98607dd6fe8ec85bb86cc65ee089f8268f");
        }

        private static void Case_03323()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3323,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,11,79,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,20,8,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-8,63,5,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "84891ecc4b9b29d8f8c502821d42e37aa0b8cc36d831eee1aa3419c98ab586d3");
        }

        private static void Case_03324()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3324,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-20,19,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-1,41,2,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "1c08f641237731b37e7bf3816a129191b2faa9aeb4c56b3aac14d3c971958336");
        }

        private static void Case_03325()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3325,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-8,42,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-15,65,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-7,81,43,4), new GeneratedEnemyUnit(5,0,13,24,3), new GeneratedEnemyUnit(-13,9,60,43,4), new GeneratedEnemyUnit(18,2,84,22,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "65468ee947c8beac180e3e68e49a2fd203531ee2596a1baa1e338f0e1c5f7b75");
        }

        private static void Case_03326()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3326,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,20,98,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,19,17,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,15,84,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-1,49,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,17,93,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-6,77,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,10,13,50,2), new GeneratedEnemyUnit(4,6,32,4,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "63288893f84c5602c614384cd7db44201813dc9ce8e178b01e5d8fe20b245971");
        }

        private static void Case_03327()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3327,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,16,65,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-17,42,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,2,48,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,10,66,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,18,90,23,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 75,
                stableHash: "d9b7f04ebdb1b298c7bd168b52e92915212fc5640c1ae34bd8b7579c26e86f33");
        }

        private static void Case_03328()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3328,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-18,21,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,13,53,8,3), new GeneratedEnemyUnit(17,-3,20,8,2), new GeneratedEnemyUnit(10,17,30,17,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "b7e863f00ffbcb57209cbbb9d3c328ed7027933af3bba214bcfd7d2201af7800");
        }

        private static void Case_03329()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3329,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-11,6,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,13,79,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,3,7,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-5,30,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,13,54,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-20,22,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-6,71,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-13,92,11,3), new GeneratedEnemyUnit(-8,5,31,2,3), new GeneratedEnemyUnit(-20,-11,96,7,1), new GeneratedEnemyUnit(13,-1,56,26,1), new GeneratedEnemyUnit(-6,11,87,8,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "bee81cedc550943d847af0ab52994f0213478d73c853887ac763fd23d399a4ea");
        }

        private static void Case_03330()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3330,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,20,12,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-12,95,35,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "9a5a1a8bbe1d799efb694fbd83a8d0e6466baaaeeabdbe77389e8ed9063c46e3");
        }

        private static void Case_03331()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3331,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-11,46,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,19,73,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-3,90,18,4), new GeneratedEnemyUnit(9,13,42,12,3), new GeneratedEnemyUnit(-15,-3,75,46,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "df939155470cbac0c19dd6d8474973d67617ccf194a37300656f1b57e25207ca");
        }

        private static void Case_03332()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3332,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-3,14,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-15,51,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,13,100,37,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "8380aa34491f98b04e24777b290f9cf38abdaba8f26979aea39fa0baf9347818");
        }

        private static void Case_03333()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3333,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,6,15,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-11,81,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,12,88,31,1), new GeneratedEnemyUnit(1,-10,43,31,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "437de335f5e02332e610804e64553e380a277c0aaf8ee46c9b3a73e0ddc62010");
        }

        private static void Case_03334()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3334,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-18,99,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-8,62,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-8,54,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,16,68,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-8,96,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-6,32,5,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 15,
                stableHash: "8910c326e1e4ff39fee01bcf69ad287bde466bb8d077c9f0a3ec8813cba07c38");
        }

        private static void Case_03335()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3335,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-1,11,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,1,93,6,4), new GeneratedEnemyUnit(-10,-11,14,40,2), new GeneratedEnemyUnit(-2,-7,99,7,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "2efcaae1ed0a7dc6b6f9262f1e442bc12496918edf543c569500a78e77f049f1");
        }

        private static void Case_03336()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3336,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-20,91,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,3,67,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-9,79,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-14,100,32,2), new GeneratedEnemyUnit(-20,2,26,21,1), new GeneratedEnemyUnit(-10,-6,5,44,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "3f035e25a3999c0a483bee0a474c3e6090707a2be874aef3a60d1a820f9e600b");
        }

        private static void Case_03337()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3337,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-16,63,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,6,60,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,11,29,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-11,65,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,9,94,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,20,14,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-5,16,1,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "5cf4de130121b8dc15fbf3a64febf38621c8f7eb81e92a12d04053dae9fc291e");
        }

        private static void Case_03338()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3338,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-5,85,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-10,24,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,8,22,28,3), new GeneratedEnemyUnit(12,-17,61,15,2), new GeneratedEnemyUnit(-18,-14,36,10,1), new GeneratedEnemyUnit(-3,1,43,39,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "1cf3ec345a0aa8e278e1c13f4dd344dff4f6d7fbe0af643a7e526fc94db6b951");
        }

        private static void Case_03339()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3339,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-12,43,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,3,67,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,12,26,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-15,88,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-5,88,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,15,49,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-1,8,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,20,97,26,1), new GeneratedEnemyUnit(-9,5,16,49,2), new GeneratedEnemyUnit(-16,7,82,48,2), new GeneratedEnemyUnit(11,6,98,31,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "42a1a8ad26cbdfeadfd08fdf66dd3f6ef653dac018fb52f2b910c08eeacb3634");
        }

        private static void Case_03340()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3340,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-14,49,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-2,22,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,5,25,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,17,51,7,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "5c0aa3dc1f6426b1936205f367491a4fbec1c88f20b602b4c0dff5d574e7122e");
        }

        private static void Case_03341()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3341,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-20,66,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-3,6,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,5,65,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-7,88,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,3,73,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-9,86,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-15,57,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-20,22,17,1), new GeneratedEnemyUnit(-15,12,55,23,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "7ab593aab0afe31143c6e6386510ddaf24c061ed9aa167492894646b5a781b3b");
        }

        private static void Case_03342()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3342,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-9,29,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,19,73,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,10,6,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,1,87,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-19,17,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,8,12,4,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "bc828c498200a53695edd372285be66acd54e2c6ab651417989bdfe6994880b0");
        }

        private static void Case_03343()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3343,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,19,18,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-3,19,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,13,64,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-2,23,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-20,51,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-19,41,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-6,62,20,2), new GeneratedEnemyUnit(4,7,62,32,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "523d07dc0965748a253a0f5f927b2c8c2f2ea7ba4ee54c95ef36c2acc9fa5ff4");
        }

        private static void Case_03344()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3344,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-6,8,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-1,86,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,0,69,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-1,97,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-1,10,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,3,73,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-2,35,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,4,68,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,7,6,36,1), new GeneratedEnemyUnit(14,7,71,19,4), new GeneratedEnemyUnit(-12,-18,37,40,4), new GeneratedEnemyUnit(0,10,69,35,2), new GeneratedEnemyUnit(-12,-4,30,28,4), new GeneratedEnemyUnit(11,-2,56,40,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "1edcb816ee7814eda86efb044ce575f969d379a5f7398c99e9d46a6de57498a5");
        }

        private static void Case_03345()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3345,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-7,37,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-8,71,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-13,96,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,8,9,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-19,12,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,14,22,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-3,85,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-6,29,50,4), new GeneratedEnemyUnit(4,-11,36,11,4), new GeneratedEnemyUnit(-5,-20,46,23,3), new GeneratedEnemyUnit(-10,2,92,40,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "2a48503588a2405ea7a52064c75ada6566a58b45fae56c2c2bda9dd2915dcb34");
        }

        private static void Case_03346()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3346,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,19,24,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-7,100,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-15,73,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-12,6,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,7,76,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-3,9,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,9,68,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-6,53,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,9,74,37,4), new GeneratedEnemyUnit(-2,-17,94,13,1), new GeneratedEnemyUnit(20,-7,38,48,4), new GeneratedEnemyUnit(-6,-4,33,49,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "5e2ab06d7d745b0faba821edcd0bdd89c9fa4a9292a91f11d25d7ee2f6e5eaf2");
        }

        private static void Case_03347()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3347,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-1,53,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-18,81,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,20,14,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-6,10,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,12,86,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,7,84,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,4,60,48,4), new GeneratedEnemyUnit(17,4,75,42,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "e871289ffb6b0b7da5c2b721a5c13043ba280a07e4ecf9c3f08718f25946b8b6");
        }

        private static void Case_03348()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3348,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-8,9,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,1,33,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-5,21,32,1), new GeneratedEnemyUnit(-5,4,86,32,4), new GeneratedEnemyUnit(16,19,67,44,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "6467036846d36ecc6848e5ab643ceaeb6f1cd360a756c8a34c2ebe154cadce35");
        }

        private static void Case_03349()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3349,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,17,9,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,8,58,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,16,68,35,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "b2e04da6b49a130a6de545b15c87ca51d2bfb5f586587ff49fb02a79aa9f4167");
        }

        private static void Case_03350()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3350,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,5,34,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,16,73,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-7,73,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,20,7,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,13,63,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,12,28,10,3), new GeneratedEnemyUnit(-13,-14,72,37,1), new GeneratedEnemyUnit(-19,-18,74,5,4), new GeneratedEnemyUnit(17,8,96,49,2), new GeneratedEnemyUnit(-1,-17,70,22,1), new GeneratedEnemyUnit(14,4,11,10,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "96b35648719e26fcb13f05b4f2cac9a982011c589f7f5975b6b119cdf1f144c3");
        }

        private static void Case_03351()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3351,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-8,28,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,20,82,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-20,51,35,4), new GeneratedEnemyUnit(-3,0,75,28,1), new GeneratedEnemyUnit(-11,5,40,48,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "b13ab27358d573c0e9594927e18257ca8784197fc5eb4a1b0f80202460967d2f");
        }

        private static void Case_03352()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3352,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,14,18,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,9,50,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,11,50,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,6,11,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-17,94,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-18,94,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-4,9,18,3), new GeneratedEnemyUnit(9,9,81,46,2), new GeneratedEnemyUnit(3,4,9,1,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "ab6d78d4920d4ceeed2337b9fa8ed6dd1148bc52b8ec0a4e91b794f3ee9b099f");
        }

        private static void Case_03353()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3353,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-13,79,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,0,99,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,10,16,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,10,33,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-7,13,50,2), new GeneratedEnemyUnit(-11,16,57,35,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "6572b3957203ff5788de95e63f40bb6d32391ea56f3fd25f7bfa5e1f2a5338f5");
        }

        private static void Case_03354()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3354,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,4,29,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,17,91,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,11,69,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,6,77,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-10,8,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,19,50,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,9,62,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-3,79,23,3), new GeneratedEnemyUnit(12,9,84,10,3), new GeneratedEnemyUnit(17,15,15,16,2), new GeneratedEnemyUnit(-8,17,45,17,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "d42239feb32f5ba5e983da721cc04d2f2c663698a63c2ba35ec1c6c178d5baec");
        }

        private static void Case_03355()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3355,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-14,80,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-19,76,3,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "9160e50f2fae81da30a8535b459a5895ab79a4b1e408c231ed93d957c523a6b0");
        }

        private static void Case_03356()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3356,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,17,59,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-15,83,24,1), new GeneratedEnemyUnit(-12,-13,79,44,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "43a99de5ce5454afa36136bb11f55b1fd46af836c66b97aa1963d9199efdc6cc");
        }

        private static void Case_03357()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3357,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-17,34,7,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "f16c5d5dcc1ea7052b1ea3430489cf5a4ae9e13a20a1a8c86b0bd4adad584d00");
        }

        private static void Case_03358()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3358,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,0,77,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,0,56,19,2), new GeneratedEnemyUnit(4,18,78,33,2), new GeneratedEnemyUnit(-13,-17,62,8,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "d9e518361d59c92df425005e825c7719fe3344644a336f612416fe0c11951c56");
        }

        private static void Case_03359()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3359,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,18,76,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,1,8,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-19,73,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-16,24,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-17,89,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,19,52,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,5,80,10,3), new GeneratedEnemyUnit(-20,18,20,1,4), new GeneratedEnemyUnit(-10,14,26,50,3), new GeneratedEnemyUnit(-8,4,51,9,4), new GeneratedEnemyUnit(8,-8,8,16,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "1d6f9c8ab281cb8213518458dedf632e78923ab216218f82d999c53d504569e7");
        }

        private static void Case_03360()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3360,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-10,59,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-4,65,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-13,44,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-2,51,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,17,15,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,5,14,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,3,78,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,19,95,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,8,48,1,3), new GeneratedEnemyUnit(4,-20,69,14,1), new GeneratedEnemyUnit(-6,3,82,35,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "c232a2fce4690712a1b230c8a09cd5079338ccf606bd619cf0dbaceca793951e");
        }

        private static void Case_03361()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3361,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,6,55,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-15,31,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-2,98,5,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "07c4e08b729b6a9093809fe505f80d2574908de1f57467a0aa9f5a61ea8e6499");
        }

        private static void Case_03362()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3362,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,1,35,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-7,95,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-15,66,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-12,25,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-11,76,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,11,7,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,18,28,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,19,9,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,17,44,1,4), new GeneratedEnemyUnit(-3,-19,27,8,4), new GeneratedEnemyUnit(10,-11,81,24,3), new GeneratedEnemyUnit(-5,-7,18,5,2), new GeneratedEnemyUnit(-6,1,66,45,3), new GeneratedEnemyUnit(3,-10,44,32,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "091866dd6140ab0a474416af0d2187f8af527c38b5701d410a7d159a9a135997");
        }

        private static void Case_03363()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3363,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-11,44,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-14,58,5,1), new GeneratedEnemyUnit(5,-10,7,34,4), new GeneratedEnemyUnit(16,-3,18,25,1), new GeneratedEnemyUnit(-16,19,49,48,1), new GeneratedEnemyUnit(-15,14,28,48,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "f164aa89cfb8270178377f93a4b0a21b6a6a1488788bc5fe7a5221859b3acddc");
        }

        private static void Case_03364()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3364,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-7,51,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-2,19,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-6,28,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,15,29,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,12,93,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,4,48,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-5,12,5,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "7711e98ec238b6ac5baec8e9d72e0417a259247ec00f97708eab746bc8709780");
        }

        private static void Case_03365()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3365,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-8,90,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-3,70,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,7,31,7,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "64bb21d9eab785fc0d7f2aff71335e654eb9785b899d5c94be286a34fa9d3072");
        }

        private static void Case_03366()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3366,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-17,58,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-4,61,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-1,20,43,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "fd79bfe694d707cc66c70b78b02115049dff7a08cce6f61a160caa1ee36bc23f");
        }

        private static void Case_03367()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3367,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-7,85,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,6,27,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-18,39,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,19,71,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-11,67,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-2,58,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-8,83,7,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,10,40,7,4), new GeneratedEnemyUnit(-2,11,25,16,3), new GeneratedEnemyUnit(-16,-15,13,50,3), new GeneratedEnemyUnit(19,-1,96,33,3), new GeneratedEnemyUnit(19,4,76,33,4), new GeneratedEnemyUnit(-16,-6,73,9,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "5f67f892a97a23b9c72bd0669a3e71f221ab17cb72c428e8148ec1c1609f191e");
        }

        private static void Case_03368()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3368,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,19,20,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,2,34,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-3,89,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-11,14,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,12,80,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,7,57,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,1,90,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,20,56,9,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "92b36d0a57275f17b6eb272aa6abc4e672aa50fa90d2667283707c6f0679026b");
        }

        private static void Case_03369()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3369,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-7,50,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,14,38,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,19,22,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-16,19,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,0,42,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,6,52,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,9,93,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-5,20,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,0,31,41,3), new GeneratedEnemyUnit(10,-11,11,31,3), new GeneratedEnemyUnit(9,-9,46,9,1), new GeneratedEnemyUnit(-7,-14,99,20,3), new GeneratedEnemyUnit(5,16,82,6,1), new GeneratedEnemyUnit(13,-5,37,5,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "e672ef489a0fcf3193d5c3d1d94aa5d967254c1aa344a52f75c36b63233ec331");
        }

        private static void Case_03370()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3370,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-16,37,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,5,50,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,19,14,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,4,74,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-10,39,36,1), new GeneratedEnemyUnit(-20,-12,88,20,4), new GeneratedEnemyUnit(15,16,10,48,1), new GeneratedEnemyUnit(-16,-19,74,14,2), new GeneratedEnemyUnit(-8,19,15,4,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "e958d6ec97039cb7a2044ae2bc26216e4a7be06daf625ceea2717386e9a930b6");
        }

        private static void Case_03371()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3371,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,16,94,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,4,50,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-2,73,6,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "feb606adad99e39b8ba84e6afd921806154d9c96471eef5185642b7c86801bac");
        }

        private static void Case_03372()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3372,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,7,47,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,1,17,1,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 15,
                stableHash: "7462845968b121ec0a5c76d88cc8e43ba30c49f1ad77ce63408a1728f221a445");
        }

        private static void Case_03373()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3373,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-14,42,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,19,34,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-6,55,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-3,11,35,3), new GeneratedEnemyUnit(-20,-2,26,44,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "93042a42269847617fb081bf07c63f55f5cd68ecf49830b7c155d59a41660b3e");
        }

        private static void Case_03374()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3374,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-1,67,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,12,76,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-7,79,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,0,62,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-18,17,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,3,21,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,12,21,26,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "bca8625ede9726355cb9853a35c3fc57a36ce1e2ea62b5f81285e26a47ce6bec");
        }

        private static void Case_03375()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3375,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-11,48,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,1,10,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,19,98,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,1,70,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,18,82,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-19,50,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-14,36,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,0,76,23,4), new GeneratedEnemyUnit(9,-7,32,40,2), new GeneratedEnemyUnit(16,-12,63,45,3), new GeneratedEnemyUnit(-14,7,86,8,4), new GeneratedEnemyUnit(2,3,93,39,2), new GeneratedEnemyUnit(-19,6,39,7,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "7b646492476f8d1e2272cc70447c5ee50a4c40e6213d92b27b0bb6e810bdd921");
        }

        private static void Case_03376()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3376,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,11,50,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-20,28,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,19,44,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-12,49,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,2,31,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,13,92,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-3,6,28,2), new GeneratedEnemyUnit(8,-4,6,43,2), new GeneratedEnemyUnit(-2,-6,79,3,2), new GeneratedEnemyUnit(-16,1,58,9,2), new GeneratedEnemyUnit(-16,-3,82,17,3), new GeneratedEnemyUnit(-4,12,22,1,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "814a0f89cc063b1b3b73346144c438545a9900f50c42e006536bbe7a095fd980");
        }

        private static void Case_03377()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3377,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-9,26,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,2,5,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-10,57,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,12,98,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,20,44,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,11,49,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-20,67,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,17,5,17,4), new GeneratedEnemyUnit(8,-8,68,3,1), new GeneratedEnemyUnit(4,-3,52,3,2), new GeneratedEnemyUnit(-3,11,42,41,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "725a84f7d1745a043437ca00eacb11d63b825a0765dbc1ece8e0bcb2c43f3436");
        }

        private static void Case_03378()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3378,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,18,34,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,13,67,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,1,38,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-17,97,43,1), new GeneratedEnemyUnit(8,19,98,8,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "2d6e9f300f8acbf24b1d4990891e3a5c3ccbb6cfdd4e09b2bcec2d650524bb44");
        }

        private static void Case_03379()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3379,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,20,7,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,0,68,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,6,60,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,12,6,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-5,89,49,4), new GeneratedEnemyUnit(6,18,19,41,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "82e76113f419552c72f3cc735c321f679f7aa4c0aa4452479e5fa385913a3336");
        }

        private static void Case_03380()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3380,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,5,58,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-14,58,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,2,22,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-10,38,10,3), new GeneratedEnemyUnit(16,-15,11,22,4), new GeneratedEnemyUnit(6,8,33,6,1), new GeneratedEnemyUnit(9,13,43,8,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "e3432b3a6eecc30e58d46dbc2876b54bc2965f26839225238489275e0bfad4ac");
        }

        private static void Case_03381()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3381,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,1,39,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,4,11,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-3,65,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,12,21,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,9,5,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,11,74,37,1), new GeneratedEnemyUnit(-6,-20,55,47,2), new GeneratedEnemyUnit(-16,9,9,48,1), new GeneratedEnemyUnit(7,8,65,26,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "1c33a4f58d5a26b016c16adfea4da4b1a684cad13f1cafe43745884684c6bd0d");
        }

        private static void Case_03382()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3382,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,14,56,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,2,19,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-7,88,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-9,21,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,7,13,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,19,23,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,17,34,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,8,52,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,17,58,36,4), new GeneratedEnemyUnit(17,-3,38,46,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "1d3068187311f98c0fcea5443c56c728b77e8a840f8a9cacfcc0d0ba5ff546e8");
        }

        private static void Case_03383()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3383,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-7,82,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-18,24,3,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,15,27,37,2), new GeneratedEnemyUnit(17,4,7,5,3), new GeneratedEnemyUnit(-8,-14,90,28,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "ef101e163b27a254717ea70cc5fe0d019f37c08d5352aa92354e033d0e2355c0");
        }

        private static void Case_03384()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3384,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,17,51,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,9,74,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,16,7,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-13,72,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-14,17,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,4,64,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-1,80,46,4), new GeneratedEnemyUnit(-18,11,59,43,1), new GeneratedEnemyUnit(-1,-10,43,9,2), new GeneratedEnemyUnit(-5,20,18,3,1), new GeneratedEnemyUnit(15,0,82,13,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "a0f9b645eb3a8424d0604395d37bb04020362a5fd2962f940c2695830a370214");
        }

        private static void Case_03385()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3385,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-4,17,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-20,36,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,1,49,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-1,64,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,7,88,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,10,95,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-7,64,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-20,84,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,0,30,26,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "ccc42d104d8b7fc97ab8b845c83a4e97985a848e75344ae4d76f8cca570691e5");
        }

        private static void Case_03386()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3386,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,8,82,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-10,75,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,11,11,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-11,71,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,14,98,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,14,53,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,12,74,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,0,58,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-10,77,38,4), new GeneratedEnemyUnit(6,6,65,16,3), new GeneratedEnemyUnit(2,-2,92,31,1), new GeneratedEnemyUnit(5,-5,49,45,1), new GeneratedEnemyUnit(2,8,75,33,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "ce80abb80af4c4a921a7977ff79e06ad5037ab94aaca149469688a256adc8658");
        }

        private static void Case_03387()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3387,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,15,43,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-4,59,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,14,46,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-14,77,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-15,41,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,20,79,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-8,60,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,1,66,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,14,6,29,1), new GeneratedEnemyUnit(-10,11,88,3,4), new GeneratedEnemyUnit(13,12,42,7,4), new GeneratedEnemyUnit(11,-7,79,21,3), new GeneratedEnemyUnit(-13,-4,13,42,1), new GeneratedEnemyUnit(-3,2,26,18,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 35,
                stableHash: "e37f047608ac083e17153905e442fbb81fd3049f1541f07c49eb8f20b59f939e");
        }

        private static void Case_03388()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3388,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,4,79,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-15,7,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-15,22,13,2), new GeneratedEnemyUnit(-5,-7,89,40,1), new GeneratedEnemyUnit(-13,19,81,23,4), new GeneratedEnemyUnit(13,11,72,7,1), new GeneratedEnemyUnit(-1,-14,24,20,2), new GeneratedEnemyUnit(-2,-5,48,15,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "6ba9d8604a8aaecffa6be591fb32f83e9a78fd62457eab2069c8208e4a610738");
        }

        private static void Case_03389()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3389,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,19,8,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,20,37,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,9,10,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,19,46,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,15,21,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-1,14,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,20,36,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-5,43,50,1), new GeneratedEnemyUnit(-18,-3,13,11,1), new GeneratedEnemyUnit(2,11,42,40,4), new GeneratedEnemyUnit(-4,-16,54,38,3), new GeneratedEnemyUnit(-14,-1,29,45,1), new GeneratedEnemyUnit(-19,18,96,5,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "6f4e2d7a51ebf66d542b94c74e6400966b27f89ccda5bf61adf9a0f9253304fb");
        }

        private static void Case_03390()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3390,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-18,38,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,4,80,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,9,89,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,4,44,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,7,24,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-2,35,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-3,81,6,1), new GeneratedEnemyUnit(4,-3,72,5,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "8f86101f9056f7bef96ab7a9e2c4c6ea2afb7a6fce405fa9525e6b176291e812");
        }

        private static void Case_03391()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3391,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-5,94,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,20,40,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,9,10,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,16,6,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-4,95,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-4,55,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-16,72,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-7,55,22,1), new GeneratedEnemyUnit(20,9,99,22,4), new GeneratedEnemyUnit(4,18,47,28,1), new GeneratedEnemyUnit(10,17,39,35,4), new GeneratedEnemyUnit(-9,-12,62,29,4), new GeneratedEnemyUnit(-20,-17,98,17,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "516b44cb3e2e0e9a571357db1ab07ab9fb3098b1b33ae9371edf3fef31355fd5");
        }

        private static void Case_03392()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3392,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-3,87,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,16,50,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,7,75,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-16,43,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,14,55,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-7,35,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,7,37,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-14,15,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,11,61,9,4), new GeneratedEnemyUnit(20,17,85,38,2), new GeneratedEnemyUnit(-10,9,42,30,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "84b4fdc1bb26bc5479aa3b1be7bbd413e764a3d01abab6295b17a60e754f149a");
        }

        private static void Case_03393()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3393,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,16,80,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,6,100,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-15,58,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,10,85,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,1,77,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,15,100,32,2), new GeneratedEnemyUnit(10,17,58,9,4), new GeneratedEnemyUnit(-15,7,9,10,2), new GeneratedEnemyUnit(-1,-14,44,49,2), new GeneratedEnemyUnit(2,-9,90,35,2), new GeneratedEnemyUnit(16,-12,55,26,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "de2f5f7b87b831f779893ba2182c3459e2e1ac2491ef9f254d6643b19012bc57");
        }

        private static void Case_03394()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3394,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-1,58,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-1,32,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,6,54,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,13,20,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,5,35,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,18,38,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,13,49,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-20,87,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-11,10,25,4), new GeneratedEnemyUnit(0,-20,46,36,3), new GeneratedEnemyUnit(-15,4,21,33,4), new GeneratedEnemyUnit(-10,13,93,28,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "112054f0c5b2b5e654f7e1ab28f0c2ff2e90034f125158ba3c82927b294edb48");
        }

        private static void Case_03395()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3395,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,9,9,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,0,24,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-10,12,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,12,48,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-9,45,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-3,60,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,14,63,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,9,57,8,3), new GeneratedEnemyUnit(-18,-12,44,10,2), new GeneratedEnemyUnit(13,-17,86,10,2), new GeneratedEnemyUnit(9,1,56,15,2), new GeneratedEnemyUnit(-16,-3,45,18,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "7d72b31641bcd70f3e56bc9560ad8c7a966b44a0ff0dacf3b857ca7681490170");
        }

        private static void Case_03396()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3396,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,10,9,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,16,41,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-5,63,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,18,37,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-14,76,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-5,34,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-14,46,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-13,60,18,2), new GeneratedEnemyUnit(12,15,28,5,3), new GeneratedEnemyUnit(-9,-3,85,4,3), new GeneratedEnemyUnit(16,8,28,50,4), new GeneratedEnemyUnit(14,-19,36,20,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "7f96732a42abd7d8c9cd6eccfc5fbd7d3ad60781317878831a560101084aa9c0");
        }

        private static void Case_03397()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3397,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,19,48,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,17,10,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,13,87,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,0,13,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,4,28,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-17,81,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,2,70,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-17,100,46,2), new GeneratedEnemyUnit(-4,16,87,50,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "aea72bd6151522970621b9629744b4d4f55a75d67cdc96f38660c406d3513101");
        }

        private static void Case_03398()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3398,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-11,30,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-13,37,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-17,50,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-6,64,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,4,49,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,8,79,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-19,9,1,3), new GeneratedEnemyUnit(-19,-17,21,11,1), new GeneratedEnemyUnit(20,4,69,20,4), new GeneratedEnemyUnit(-16,5,12,49,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "a566ba744239fc4709962acf6c728930293d1ecd6d8041c450cd5d42e13752e6");
        }

        private static void Case_03399()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3399,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-19,37,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,18,50,45,4), new GeneratedEnemyUnit(0,-12,21,37,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "0b3673764543e7e249e2e9e04811d17d9b956b8a5bb17a0add342eafd301db63");
        }

        private static void Case_03400()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3400,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-13,6,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,1,82,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,16,97,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-15,94,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,16,39,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-12,95,2,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "1abb1cbefafbab4f304d90485f6b229705d4224cd339cc1f3e13e778271c9ae7");
        }

        private static void Case_03401()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3401,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,4,86,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-9,77,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-10,91,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,5,68,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,3,71,13,3), new GeneratedEnemyUnit(6,18,46,13,1), new GeneratedEnemyUnit(17,14,52,34,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "4f67d108e4babba2681ccb2ff4b2facebaf6a9f31e05d56364ccbe54239beb86");
        }

        private static void Case_03402()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3402,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-5,54,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-6,94,4,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "2f7ffce1ae2615a48958ceaaaef8ede8842b5359330674eb9aee77f9638f46b9");
        }

        private static void Case_03403()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3403,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,17,8,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-19,61,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-9,60,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,12,15,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,4,54,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,0,98,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-4,51,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-18,49,40,4), new GeneratedEnemyUnit(0,3,22,20,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "e32ff0de638a4185df8553a2ddc53f08f8cb3860018d00522c26a729aec12e78");
        }

        private static void Case_03404()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3404,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-15,21,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,17,40,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-17,27,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-7,74,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,8,45,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,11,69,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-16,67,4,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "2527ddd720a519a5d238c1ac3c874afd517db4cf295a5eae0ee73b353b0a7a06");
        }

        private static void Case_03405()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3405,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-14,86,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,5,57,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-2,11,27,3), new GeneratedEnemyUnit(-4,-16,35,24,1), new GeneratedEnemyUnit(-15,8,90,9,3), new GeneratedEnemyUnit(4,19,33,7,4), new GeneratedEnemyUnit(-1,2,38,30,4), new GeneratedEnemyUnit(1,-14,52,24,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "e8154b9fa493fa5a27dd860c1b9741cc5f02259dc688589ac11e200d8fac318a");
        }

        private static void Case_03406()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3406,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-6,84,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,20,65,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-2,89,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,8,66,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,11,24,25,2), new GeneratedEnemyUnit(16,15,18,40,3), new GeneratedEnemyUnit(4,-4,84,27,3), new GeneratedEnemyUnit(-18,-11,64,31,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "fa9fbe5141beffbf6bfda59a3fa6518f7d281276ef0a013269125f29d7284730");
        }

        private static void Case_03407()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3407,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-11,75,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-17,9,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-4,22,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,3,40,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,13,11,43,2), new GeneratedEnemyUnit(-2,-13,94,30,4), new GeneratedEnemyUnit(-15,-18,41,50,4), new GeneratedEnemyUnit(-19,-11,80,40,3), new GeneratedEnemyUnit(20,6,49,15,1), new GeneratedEnemyUnit(1,14,40,29,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "e83e2581a5fbb45ab1bee5b88ee761a9f7a6e1e5c78a0b7d24d667d1ca995dcd");
        }

        private static void Case_03408()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3408,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-10,84,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,13,64,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,11,97,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-15,91,22,1), new GeneratedEnemyUnit(-13,8,73,46,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "8232e8542c0707cd4d05b711b15872aab5d0d39cde23b4055ef236573e65bf8b");
        }

        private static void Case_03409()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3409,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,3,68,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-6,84,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,10,18,1,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "ea68f08da7be71555bbf2b3f387ecd0d29269222ecfd25f591748d767b39a2d5");
        }

        private static void Case_03410()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3410,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,5,42,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-12,15,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,9,58,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-6,56,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-12,8,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,0,33,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,12,64,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-3,65,15,1), new GeneratedEnemyUnit(-17,15,38,44,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "6c3458ec9341254053e4ec649699aa26418af1c5a5396735a4ddff60f76e0bc4");
        }

        private static void Case_03411()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3411,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,7,48,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,5,11,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-20,79,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,8,44,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,1,9,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-3,6,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-20,49,14,4), new GeneratedEnemyUnit(-12,17,41,8,1), new GeneratedEnemyUnit(14,-16,8,41,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "5920964225a523f73c4a35d75e66a550b33b18049f44ad3bb13d036eabfd06f9");
        }

        private static void Case_03412()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3412,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,12,39,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-15,32,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,13,77,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,5,9,7,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-1,92,8,4), new GeneratedEnemyUnit(13,-13,70,2,1), new GeneratedEnemyUnit(4,13,26,28,1), new GeneratedEnemyUnit(10,6,34,21,3), new GeneratedEnemyUnit(0,19,96,16,1), new GeneratedEnemyUnit(3,8,26,13,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "f955bfb13ed336c3e674a2c09a270ea32c486953866433b660664795620b0134");
        }

        private static void Case_03413()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3413,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-17,43,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,19,96,25,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "e8faa0e80277a93959f9e8b0af2aa07089fc99dc3f46cd81cb62c76c12a4bfa7");
        }

        private static void Case_03414()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3414,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,7,20,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-15,77,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,0,38,5,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "a57f02f507e4e767a57a29805ce67bdde0247532c525e48e37c5b1389ff4a415");
        }

        private static void Case_03415()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3415,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,18,67,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,5,10,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,5,66,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,7,14,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-10,29,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-20,18,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,2,57,21,4), new GeneratedEnemyUnit(7,-10,34,28,2), new GeneratedEnemyUnit(10,-16,93,6,2), new GeneratedEnemyUnit(0,19,82,26,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "121217a98ab6ba3a06386f01c7df4443fd3f558ce0fd489a26479a9cb488d68a");
        }

        private static void Case_03416()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3416,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-13,86,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-10,71,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,4,23,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,3,16,38,2), new GeneratedEnemyUnit(-20,9,65,1,4), new GeneratedEnemyUnit(13,6,21,42,3), new GeneratedEnemyUnit(-6,2,100,48,2), new GeneratedEnemyUnit(3,-3,78,38,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "9b68842e981fc7d97ec0fca18a6c71ee153468c6502fb5049b67d4ec2680247a");
        }

        private static void Case_03417()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3417,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-18,79,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,14,9,22,2), new GeneratedEnemyUnit(17,-15,98,7,3), new GeneratedEnemyUnit(-11,-4,82,32,1), new GeneratedEnemyUnit(-5,18,42,44,1), new GeneratedEnemyUnit(-10,1,52,30,4), new GeneratedEnemyUnit(11,-19,69,33,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "26e8ae309b267b55268d2edeb76d4ab514d49568433d04cd98f2f9349bdd7f22");
        }

        private static void Case_03418()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3418,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-11,30,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-15,85,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,9,38,35,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "3d31785e9c6143882c0ca9f1dfdb729f77b661a65ff3763cb44cd2ff0977e1ec");
        }

        private static void Case_03419()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3419,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-3,47,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-19,28,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,9,38,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-15,54,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,11,43,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-2,34,38,4), new GeneratedEnemyUnit(17,14,20,26,2), new GeneratedEnemyUnit(13,-18,67,9,3), new GeneratedEnemyUnit(-8,15,17,6,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "5eca2d9a955738d9749cfa39b0b9a39df6716818149ec3c7d9b094a9237ec794");
        }

    }
}
