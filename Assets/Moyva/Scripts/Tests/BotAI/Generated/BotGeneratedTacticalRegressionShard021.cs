using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard021
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_03780();
            Case_03781();
            Case_03782();
            Case_03783();
            Case_03784();
            Case_03785();
            Case_03786();
            Case_03787();
            Case_03788();
            Case_03789();
            Case_03790();
            Case_03791();
            Case_03792();
            Case_03793();
            Case_03794();
            Case_03795();
            Case_03796();
            Case_03797();
            Case_03798();
            Case_03799();
            Case_03800();
            Case_03801();
            Case_03802();
            Case_03803();
            Case_03804();
            Case_03805();
            Case_03806();
            Case_03807();
            Case_03808();
            Case_03809();
            Case_03810();
            Case_03811();
            Case_03812();
            Case_03813();
            Case_03814();
            Case_03815();
            Case_03816();
            Case_03817();
            Case_03818();
            Case_03819();
            Case_03820();
            Case_03821();
            Case_03822();
            Case_03823();
            Case_03824();
            Case_03825();
            Case_03826();
            Case_03827();
            Case_03828();
            Case_03829();
            Case_03830();
            Case_03831();
            Case_03832();
            Case_03833();
            Case_03834();
            Case_03835();
            Case_03836();
            Case_03837();
            Case_03838();
            Case_03839();
            Case_03840();
            Case_03841();
            Case_03842();
            Case_03843();
            Case_03844();
            Case_03845();
            Case_03846();
            Case_03847();
            Case_03848();
            Case_03849();
            Case_03850();
            Case_03851();
            Case_03852();
            Case_03853();
            Case_03854();
            Case_03855();
            Case_03856();
            Case_03857();
            Case_03858();
            Case_03859();
            Case_03860();
            Case_03861();
            Case_03862();
            Case_03863();
            Case_03864();
            Case_03865();
            Case_03866();
            Case_03867();
            Case_03868();
            Case_03869();
            Case_03870();
            Case_03871();
            Case_03872();
            Case_03873();
            Case_03874();
            Case_03875();
            Case_03876();
            Case_03877();
            Case_03878();
            Case_03879();
            Case_03880();
            Case_03881();
            Case_03882();
            Case_03883();
            Case_03884();
            Case_03885();
            Case_03886();
            Case_03887();
            Case_03888();
            Case_03889();
            Case_03890();
            Case_03891();
            Case_03892();
            Case_03893();
            Case_03894();
            Case_03895();
            Case_03896();
            Case_03897();
            Case_03898();
            Case_03899();
            Case_03900();
            Case_03901();
            Case_03902();
            Case_03903();
            Case_03904();
            Case_03905();
            Case_03906();
            Case_03907();
            Case_03908();
            Case_03909();
            Case_03910();
            Case_03911();
            Case_03912();
            Case_03913();
            Case_03914();
            Case_03915();
            Case_03916();
            Case_03917();
            Case_03918();
            Case_03919();
            Case_03920();
            Case_03921();
            Case_03922();
            Case_03923();
            Case_03924();
            Case_03925();
            Case_03926();
            Case_03927();
            Case_03928();
            Case_03929();
            Case_03930();
            Case_03931();
            Case_03932();
            Case_03933();
            Case_03934();
            Case_03935();
            Case_03936();
            Case_03937();
            Case_03938();
            Case_03939();
            Case_03940();
            Case_03941();
            Case_03942();
            Case_03943();
            Case_03944();
            Case_03945();
            Case_03946();
            Case_03947();
            Case_03948();
            Case_03949();
            Case_03950();
            Case_03951();
            Case_03952();
            Case_03953();
            Case_03954();
            Case_03955();
            Case_03956();
            Case_03957();
            Case_03958();
            Case_03959();
        }

        private static void Case_03780()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3780,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-16,66,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,7,41,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-5,93,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,10,38,6,4), new GeneratedEnemyUnit(9,18,18,45,2), new GeneratedEnemyUnit(1,12,53,39,1), new GeneratedEnemyUnit(-6,20,40,1,1), new GeneratedEnemyUnit(12,14,89,36,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "a9c8e33bbd372950240a1a4bc7c101f6f7afda8c52789020365990cb71eb657d");
        }

        private static void Case_03781()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3781,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-18,5,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-6,71,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-7,17,9,1), new GeneratedEnemyUnit(-6,19,90,30,4), new GeneratedEnemyUnit(-5,-12,90,29,4), new GeneratedEnemyUnit(-19,13,79,37,1), new GeneratedEnemyUnit(12,-7,84,45,2), new GeneratedEnemyUnit(-14,10,51,14,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "8ed41209eb4cf04842ee71c9bcac994cee5727085b281ac5b2c2d8760067a69c");
        }

        private static void Case_03782()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3782,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-5,69,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-20,37,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-15,83,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-10,58,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,3,88,45,3), new GeneratedEnemyUnit(8,-9,96,7,1), new GeneratedEnemyUnit(13,11,68,25,3), new GeneratedEnemyUnit(-2,-15,71,22,2), new GeneratedEnemyUnit(5,10,51,42,3), new GeneratedEnemyUnit(-20,-1,30,48,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 40,
                stableHash: "554c31e46475634f4d11e8fe4a4c32a796c330f360e0ae29732ad4380c4e662a");
        }

        private static void Case_03783()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3783,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-10,51,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-13,28,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,7,73,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,0,76,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,14,51,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,19,87,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,4,70,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,6,58,39,4), new GeneratedEnemyUnit(16,15,60,10,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "d12f681f97a94aabd91ec741dd2b4319662c1a6b8047636d8487459214538fd1");
        }

        private static void Case_03784()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3784,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-20,62,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-19,70,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-7,93,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-11,84,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-10,22,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,15,80,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,3,13,13,2), new GeneratedEnemyUnit(-13,-1,65,14,4), new GeneratedEnemyUnit(8,19,8,41,4), new GeneratedEnemyUnit(-7,15,93,10,4), new GeneratedEnemyUnit(-17,-18,67,33,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "cf295361ace715251d02b0795c7fde14771efc5ff8b6ec78e8b7002b59f3220c");
        }

        private static void Case_03785()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3785,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,10,8,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-13,28,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,7,90,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,5,52,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-9,89,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,5,84,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-17,82,38,4), new GeneratedEnemyUnit(20,20,46,23,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "26b19cad236c614f95fb5aa05dae9a0709d891fa37b98ec4f3fe6354151f732f");
        }

        private static void Case_03786()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3786,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-13,38,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-19,87,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,19,47,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,7,62,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-4,68,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,5,50,8,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "d4c9c459eb4405860a784c06c20a1c51922f0b5ed54b372a24f94cdbd2456907");
        }

        private static void Case_03787()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3787,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,19,60,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,3,90,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-15,10,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,14,93,13,3), new GeneratedEnemyUnit(16,7,84,50,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "1d27e19e09d2e2a7f9d67654aadcae52ff8709802ed5d25f25fe77a3d7b8aa31");
        }

        private static void Case_03788()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3788,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,16,91,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,20,30,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-11,36,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,1,87,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-9,60,39,4), new GeneratedEnemyUnit(5,8,32,23,1), new GeneratedEnemyUnit(3,9,43,35,1), new GeneratedEnemyUnit(12,-12,74,32,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "c9964bf0b4378c802c5ca3b85b609ae67b16fc7354bf944a56cbe773190f54de");
        }

        private static void Case_03789()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3789,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,4,12,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,9,49,49,1), new GeneratedEnemyUnit(20,12,44,44,2), new GeneratedEnemyUnit(-15,-2,61,26,1), new GeneratedEnemyUnit(-4,-12,82,11,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "84b3fb5f5582077e23e5d6a6f0881641ae5ed0cd08499f6604a9f2210a599dc2");
        }

        private static void Case_03790()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3790,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,7,51,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-6,19,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-12,31,1,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "3f30757656fad62e2f4f1f7363345228a45ac82b19e1f3cc4b7fcf33e39ca3dc");
        }

        private static void Case_03791()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3791,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,7,100,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-16,13,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-18,70,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,12,10,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,1,72,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,9,11,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,1,7,46,2), new GeneratedEnemyUnit(19,12,22,17,3), new GeneratedEnemyUnit(18,14,9,47,3), new GeneratedEnemyUnit(-6,19,52,10,2), new GeneratedEnemyUnit(-13,-19,10,5,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "77ff55b001f2b5c16ad1d5288ec52fb7063ca5551cad92b4f587756089743e88");
        }

        private static void Case_03792()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3792,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-20,32,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,3,97,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-17,8,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,0,50,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,0,60,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-15,41,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-2,79,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-3,50,50,2), new GeneratedEnemyUnit(-20,-4,64,22,1), new GeneratedEnemyUnit(6,-16,80,25,1), new GeneratedEnemyUnit(-6,-11,45,22,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "d5b1a537ab2160c46599d40c7a40b59b3a6edfb4a6cddc5d4da52ccc0f185647");
        }

        private static void Case_03793()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3793,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,5,77,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,4,27,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,14,96,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-8,56,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,9,14,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,19,77,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,15,27,36,4), new GeneratedEnemyUnit(-4,-15,97,14,1), new GeneratedEnemyUnit(-4,0,28,17,4), new GeneratedEnemyUnit(11,5,7,46,1), new GeneratedEnemyUnit(-10,18,96,10,1), new GeneratedEnemyUnit(-12,-4,46,45,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "0d1eb4c454b5ea06906d3c78b2de9d950330484ec88dc32fa854bedacd308cf9");
        }

        private static void Case_03794()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3794,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-20,47,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,3,24,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,8,62,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-9,27,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-11,86,2,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "a6ad14eae2dd40218fc8ac34adf5749ac841fa644fc04dfe233bd880368c3af4");
        }

        private static void Case_03795()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3795,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-20,92,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,8,18,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-1,76,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-7,65,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-2,11,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-13,12,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-6,22,40,2), new GeneratedEnemyUnit(-13,12,21,46,1), new GeneratedEnemyUnit(-5,-20,40,43,3), new GeneratedEnemyUnit(-12,-12,79,32,1), new GeneratedEnemyUnit(19,-7,48,34,2), new GeneratedEnemyUnit(17,-3,30,27,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "fc53d9323a07f29437607e5c1a856b1eb1dcb077a56e5a368d3eee90da830be0");
        }

        private static void Case_03796()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3796,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,7,48,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-14,10,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-6,10,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-3,11,32,3), new GeneratedEnemyUnit(6,20,79,29,2), new GeneratedEnemyUnit(-7,18,50,48,2), new GeneratedEnemyUnit(-4,9,36,35,2), new GeneratedEnemyUnit(-19,-4,55,49,3), new GeneratedEnemyUnit(-15,14,47,26,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "ce16dbecd40758783ed6bbaaab96f591f0362b0d739a26adb442d4a71a94daed");
        }

        private static void Case_03797()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3797,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,3,43,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,15,28,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,15,94,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,10,54,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,17,27,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-15,85,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,11,45,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,7,100,13,2), new GeneratedEnemyUnit(4,12,74,28,1), new GeneratedEnemyUnit(3,-9,8,7,1), new GeneratedEnemyUnit(7,2,75,31,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "178c7d1489fe454895a6fcea0ff470cf4c4468d5c6bb1ae4d5282d17c1df4217");
        }

        private static void Case_03798()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3798,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-1,88,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,3,12,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,11,5,30,1), new GeneratedEnemyUnit(17,12,45,47,2), new GeneratedEnemyUnit(4,-3,73,31,2), new GeneratedEnemyUnit(-9,-11,84,12,2), new GeneratedEnemyUnit(-15,17,77,48,2), new GeneratedEnemyUnit(5,-6,78,27,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "2fe41c27db87bd44a739c1fa910e996853e503d3a317b431155d00fbc6ac024a");
        }

        private static void Case_03799()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3799,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,19,98,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,5,39,6,4), new GeneratedEnemyUnit(-11,-2,48,28,3), new GeneratedEnemyUnit(12,20,39,10,1), new GeneratedEnemyUnit(-12,8,49,42,1), new GeneratedEnemyUnit(-15,6,46,43,4), new GeneratedEnemyUnit(-7,19,86,21,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "d5efe593001f11077a3173d3f912676b217d0ebd02ef8962d0e09b36afadd7a5");
        }

        private static void Case_03800()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3800,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,2,36,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-15,87,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,18,59,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,6,60,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-11,30,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-19,39,37,3), new GeneratedEnemyUnit(8,-13,85,40,3), new GeneratedEnemyUnit(15,20,72,50,1), new GeneratedEnemyUnit(-1,-6,74,14,4), new GeneratedEnemyUnit(-9,-6,23,24,3), new GeneratedEnemyUnit(-5,-10,21,30,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "56e1900531d252e087234e2b95d8f01c69d3516f15558dcc8e270f7088fa1cbe");
        }

        private static void Case_03801()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3801,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-11,92,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-16,57,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-4,34,41,4), new GeneratedEnemyUnit(-3,18,41,8,2), new GeneratedEnemyUnit(2,-16,89,14,3), new GeneratedEnemyUnit(-8,6,93,21,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "44b4ee2b6aacd430063d6f2786ad073de713eeb7e766bd631d0ec3883dbaa012");
        }

        private static void Case_03802()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3802,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-14,56,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,1,16,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-6,50,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-2,22,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-12,39,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,6,51,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,10,60,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,7,6,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-8,51,18,3), new GeneratedEnemyUnit(-18,-19,61,23,1), new GeneratedEnemyUnit(17,-17,5,18,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "a6b3bc3d6a3f39bc66b3c23ee1e096b131acfe48579df5b3e48213bb0f98c172");
        }

        private static void Case_03803()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3803,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,13,20,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-12,36,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,10,5,9,4), new GeneratedEnemyUnit(-16,-2,83,26,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "1bda49f0c457c1ea31f8c98c65a17d866389d923eae950956e8055106c461eab");
        }

        private static void Case_03804()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3804,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,3,26,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-11,28,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-11,37,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,0,96,39,4), new GeneratedEnemyUnit(-6,5,93,46,3), new GeneratedEnemyUnit(-20,18,40,6,2), new GeneratedEnemyUnit(-12,-14,47,23,4), new GeneratedEnemyUnit(-10,-14,65,24,3), new GeneratedEnemyUnit(5,-2,54,18,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "7ea727ae30663b38c1078f1ba791604f3d5db261f29805311dfdf13c5de25dbe");
        }

        private static void Case_03805()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3805,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-15,13,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-3,40,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-12,48,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,1,59,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,19,26,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-4,59,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,3,50,38,2), new GeneratedEnemyUnit(5,-8,84,27,1), new GeneratedEnemyUnit(-1,10,87,13,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "270a71b04e444e59d30d725b5ff623ba56803ca060b77c971a734ce32e90b7da");
        }

        private static void Case_03806()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3806,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-6,66,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-9,67,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-13,60,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-12,11,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,14,9,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-7,59,43,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "dd4727af24e843fbf5ed12f258d1ab1617e7b7667c811ee1801b60c09d97edad");
        }

        private static void Case_03807()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3807,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,10,89,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,7,19,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-8,74,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,6,91,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,9,40,7,1), new GeneratedEnemyUnit(-9,4,16,8,3), new GeneratedEnemyUnit(-6,-12,45,32,1), new GeneratedEnemyUnit(-8,4,47,8,2), new GeneratedEnemyUnit(-14,-17,73,18,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "9b3498e3bbfc7ddd5b972f2880e985ca26334c750ab1d93724a4922165cd9dcd");
        }

        private static void Case_03808()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3808,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,12,33,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,16,93,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,17,89,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,6,15,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-14,85,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-8,84,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,10,59,3,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "1ed04fb723dc2d8dc1d1797065c02f0d8321a0f9b698e6a30cebd3a6860ba4ab");
        }

        private static void Case_03809()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3809,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,5,70,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,5,68,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,5,92,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,1,14,45,4), new GeneratedEnemyUnit(17,-12,6,41,4), new GeneratedEnemyUnit(0,-17,100,39,1), new GeneratedEnemyUnit(19,16,45,41,1), new GeneratedEnemyUnit(17,1,9,3,1), new GeneratedEnemyUnit(-14,10,61,12,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "e48ed8376ae3a463d9ec629b228906639e7fa1d34da2a31b4d67a60bc850a566");
        }

        private static void Case_03810()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3810,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,11,47,2,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "b631af26ad7fd33c05a3cf7c50a6295094766e5aa775458e962a9e0250829494");
        }

        private static void Case_03811()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3811,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-9,54,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-4,50,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-3,27,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,18,32,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,6,45,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,20,16,22,1), new GeneratedEnemyUnit(11,-10,6,36,2), new GeneratedEnemyUnit(1,-14,92,5,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "cbf9723aa814776370f67811bc611b5e45c1c46bcdd0a528eff4795aa0aee1c8");
        }

        private static void Case_03812()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3812,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,2,56,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,6,81,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,4,57,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-7,24,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,16,36,43,1), new GeneratedEnemyUnit(2,11,16,33,1), new GeneratedEnemyUnit(-16,-9,98,48,3), new GeneratedEnemyUnit(7,16,19,15,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "afb0e7512fe2f2d810ca54bb0fa114231c66ae273880bca74e0c846e2e555c87");
        }

        private static void Case_03813()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3813,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,10,98,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-17,74,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-7,98,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-19,5,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,16,88,10,1), new GeneratedEnemyUnit(19,-12,17,23,4), new GeneratedEnemyUnit(-15,6,61,33,4), new GeneratedEnemyUnit(-2,-13,64,3,1), new GeneratedEnemyUnit(1,-8,11,50,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "682ac8dc153578ba2cd212ed537bc3381a1226b7146d3d16daa0c37720e155ed");
        }

        private static void Case_03814()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3814,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-2,5,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-6,52,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,0,87,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-5,59,20,4), new GeneratedEnemyUnit(7,-9,80,2,2), new GeneratedEnemyUnit(-11,-10,7,14,2), new GeneratedEnemyUnit(-19,10,71,13,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "5e2731052727b2e0e298ecfc1c956b9110ea8a2944f71b2f175d1df2608c7958");
        }

        private static void Case_03815()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3815,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,8,17,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,4,54,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,19,81,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,15,84,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,20,46,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,9,28,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,7,77,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-3,99,6,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "dce3a962fb4314444c723b3c9361154d0f3fd57dfc24aa42c3c32a2c78585ee1");
        }

        private static void Case_03816()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3816,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,2,47,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,11,62,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,15,37,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-7,35,1,2), new GeneratedEnemyUnit(-12,8,38,11,4), new GeneratedEnemyUnit(17,6,7,26,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "be8ccfbf733feab635c67ac77244f68c85d229cd971d05f46580e3f3b74063a8");
        }

        private static void Case_03817()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3817,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-5,59,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-12,19,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,3,7,3,2), new GeneratedEnemyUnit(-5,10,72,26,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "dfa6270f3ff6148a1c827169c7932d8ca56f4cbeb0f2be3819380ff407ec5720");
        }

        private static void Case_03818()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3818,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,20,26,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-8,10,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,6,73,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-7,62,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-4,97,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-1,42,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,19,70,23,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "7c87b7458cd130bea2c051aacfb1b89971029a00d3c89a6920fb0001bf12b075");
        }

        private static void Case_03819()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3819,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,17,37,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,15,71,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-1,50,38,3), new GeneratedEnemyUnit(-14,-15,7,33,1), new GeneratedEnemyUnit(-9,-6,88,41,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "9a14a83f767eb87ca90aef32f65f0f960ad039636b82dc6e2d72fb6cb96eaaca");
        }

        private static void Case_03820()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3820,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,13,33,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-18,22,49,1), new GeneratedEnemyUnit(8,6,95,18,4), new GeneratedEnemyUnit(14,-10,22,40,3), new GeneratedEnemyUnit(-6,-4,42,21,3), new GeneratedEnemyUnit(3,11,90,4,1), new GeneratedEnemyUnit(-12,6,95,39,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "971152624fad8a770bf668eff3d88f31446fdb9a8e515b472b2f332c603c7399");
        }

        private static void Case_03821()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3821,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-1,38,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-18,81,23,2), new GeneratedEnemyUnit(-3,-15,63,10,1), new GeneratedEnemyUnit(-19,7,85,39,1), new GeneratedEnemyUnit(4,2,38,33,3), new GeneratedEnemyUnit(-16,-9,83,23,2), new GeneratedEnemyUnit(11,15,72,5,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "389846cba372853695390eb429a52bd57bb1d46008d6daf5b4244d5aae28afa1");
        }

        private static void Case_03822()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3822,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,13,92,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,6,52,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,3,12,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-10,7,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,5,56,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-13,63,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-7,70,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,0,100,7,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "36cb3c8234331476cb0e267118ea72b544af9bb01dab79aa3ed8618752ac97e1");
        }

        private static void Case_03823()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3823,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-6,53,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,18,20,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-6,14,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,19,28,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-18,50,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,6,5,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,18,76,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,2,51,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-19,98,45,3), new GeneratedEnemyUnit(-9,-9,80,40,1), new GeneratedEnemyUnit(-12,-9,55,19,2), new GeneratedEnemyUnit(15,0,94,48,4), new GeneratedEnemyUnit(-14,-3,19,2,4), new GeneratedEnemyUnit(-12,2,43,21,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "52c45137406b4b723d753f53c7902ebb1a86c51bf11fc3adcf33d8b5614dfed0");
        }

        private static void Case_03824()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3824,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,0,58,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,0,48,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,18,70,7,1), new GeneratedEnemyUnit(13,15,53,18,2), new GeneratedEnemyUnit(19,-6,78,49,2), new GeneratedEnemyUnit(-17,-1,43,9,4), new GeneratedEnemyUnit(-16,0,54,3,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "dd952505e741322886aba33955947509e74a89f9a184d877258f4e209376da88");
        }

        private static void Case_03825()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3825,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-3,72,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,18,15,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,5,88,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-20,73,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,20,56,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,2,99,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,10,100,21,4), new GeneratedEnemyUnit(-17,4,81,40,2), new GeneratedEnemyUnit(-17,-7,58,17,3), new GeneratedEnemyUnit(-3,-15,17,10,2), new GeneratedEnemyUnit(-4,-3,30,1,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "145c5fd2e44b38d21fc8529fdd3ca19fb3e3d02821b7626a5bdf81c1ed576cf6");
        }

        private static void Case_03826()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3826,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,18,80,4,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,14,36,18,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "6a1c76195b695bb221077b766c7db9142f4b6a59dc96e9fa574c4dee60e8f1aa");
        }

        private static void Case_03827()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3827,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,16,39,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-8,98,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,15,43,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,13,8,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-13,70,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-1,48,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-5,73,44,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "a9a557000ef9ff3593f2d209b5af356969d0cdc9ca8b980e39e647840f4b749c");
        }

        private static void Case_03828()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3828,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,14,7,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-18,27,25,2), new GeneratedEnemyUnit(14,-20,35,25,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "881ac8bf7ad88876ddcac47105d44a7b7c56b213d43714d64d2bfb090de69120");
        }

        private static void Case_03829()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3829,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-16,51,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,8,92,11,4), new GeneratedEnemyUnit(0,19,15,17,1), new GeneratedEnemyUnit(-19,-3,96,50,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "ac0d5531feb1583aed166697626ad2f78ad463bc64e28c0ad512e98c087dc7fb");
        }

        private static void Case_03830()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3830,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-14,79,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-19,91,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,14,43,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-16,28,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-18,85,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,18,41,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,14,58,28,1), new GeneratedEnemyUnit(20,20,99,22,1), new GeneratedEnemyUnit(12,-7,34,45,3), new GeneratedEnemyUnit(2,-4,52,49,3), new GeneratedEnemyUnit(-4,-7,38,8,1), new GeneratedEnemyUnit(-16,-14,30,17,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "07ebc8da794f17547d297d9b5a8ff3c79deb076822429ec398987e83d79f4514");
        }

        private static void Case_03831()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3831,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-16,18,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,0,60,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-4,24,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,5,76,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,16,73,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-19,25,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-20,83,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,8,20,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-2,48,33,4), new GeneratedEnemyUnit(-10,-17,33,38,3), new GeneratedEnemyUnit(-15,3,85,38,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "2317dfd2aadea7f733c46237cc81e4130ccd4602c15b33d07d30b7dba87709f8");
        }

        private static void Case_03832()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3832,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-16,81,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-7,39,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-17,47,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,13,48,6,2), new GeneratedEnemyUnit(20,-9,22,30,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "c96575633cd8bd9e2e5197aaa19e2d840c71560e4a907a2e058af94ba94c87d6");
        }

        private static void Case_03833()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3833,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-9,73,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-20,13,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-12,19,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,19,11,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-8,53,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-1,49,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-20,62,24,2), new GeneratedEnemyUnit(18,19,23,45,1), new GeneratedEnemyUnit(-10,17,86,29,3), new GeneratedEnemyUnit(17,-16,81,8,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "6abcf4570dd530ac9f4bc4aae2f25a8e4b573273fa774a3a696a2d206aee475c");
        }

        private static void Case_03834()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3834,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-17,7,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,20,56,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,0,51,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-13,40,25,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "57eb6b75007230b682c6711c1df6cd0970ef1754fddf9f020134d091221afc98");
        }

        private static void Case_03835()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3835,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,12,88,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,11,22,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,20,63,38,4), new GeneratedEnemyUnit(-10,-3,10,5,4), new GeneratedEnemyUnit(-3,-8,92,46,1), new GeneratedEnemyUnit(-3,-16,81,5,3), new GeneratedEnemyUnit(-4,-17,75,13,1), new GeneratedEnemyUnit(-2,-10,92,23,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "061dda1b8d5e84c1c02909e651461d560cafe7074cbd4d569482c635a5b08a82");
        }

        private static void Case_03836()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3836,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,13,64,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-12,21,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,11,35,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-15,48,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,16,69,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-3,30,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-6,85,28,2), new GeneratedEnemyUnit(11,19,30,33,4), new GeneratedEnemyUnit(-14,-3,49,5,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "30251f47984bfbcdf71004a605b66ff854cbacc67205e1b4a742c09e27acbed8");
        }

        private static void Case_03837()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3837,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,17,88,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-7,46,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-1,13,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,1,54,13,1), new GeneratedEnemyUnit(12,-19,44,40,2), new GeneratedEnemyUnit(-18,1,28,49,3), new GeneratedEnemyUnit(-3,-5,31,43,1), new GeneratedEnemyUnit(4,0,51,35,2), new GeneratedEnemyUnit(-4,-17,33,1,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "28f73d6fcc21d51250f34ddce3d705c8b56bb2be31e96613eddee5bad649f664");
        }

        private static void Case_03838()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3838,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-13,35,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,18,80,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,2,83,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,2,51,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,11,77,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,6,20,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,5,31,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-7,36,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,2,65,29,1), new GeneratedEnemyUnit(5,-18,13,45,2), new GeneratedEnemyUnit(12,13,85,5,2), new GeneratedEnemyUnit(9,3,97,22,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "ee477a29b021fcfc1f4c94b32e02d87051b6fa632679d69b88ce252004645cdf");
        }

        private static void Case_03839()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3839,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,7,5,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-1,92,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-15,23,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-5,90,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-1,90,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-8,66,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-4,29,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,1,36,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-9,81,29,4), new GeneratedEnemyUnit(1,16,97,14,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "03bcab02115613fb7371901f7c2cb75a4c825a977efcf7a1fb33a7ad59ad36ce");
        }

        private static void Case_03840()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3840,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-16,16,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,10,12,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-7,65,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-11,35,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,14,63,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-4,40,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,4,8,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,17,65,49,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "87ce7ea6d86afe62cb3d0b2c2a625105ee421c9954c7067568bb5aaceeded586");
        }

        private static void Case_03841()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3841,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,6,42,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-4,96,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,15,87,3,4), new GeneratedEnemyUnit(16,-6,86,5,2), new GeneratedEnemyUnit(-10,-18,20,48,3), new GeneratedEnemyUnit(10,4,18,25,2), new GeneratedEnemyUnit(-10,12,5,1,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "be01305d9e7e75671959f32447c660f6bff71c89908183b9a02bae6ceca89f0e");
        }

        private static void Case_03842()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3842,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-7,24,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-7,73,5,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "6dd65b75677a550257a376bbd971cfc2d758a9e2c04f897a1b40af7d7532a23c");
        }

        private static void Case_03843()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3843,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,0,88,7,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-1,93,32,4), new GeneratedEnemyUnit(18,5,51,32,1), new GeneratedEnemyUnit(12,14,23,36,1), new GeneratedEnemyUnit(7,-3,89,10,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "79b65f810390354b81348b2332eb251bea0e02972b30a09df6b4c4f249abf961");
        }

        private static void Case_03844()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3844,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-16,12,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,4,19,18,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "299787303067fcb28217212576f870ad895fe48e854567314bb2b4406ab097dd");
        }

        private static void Case_03845()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3845,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-17,73,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,1,38,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-2,45,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-12,64,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,10,85,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,8,53,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,8,23,50,3), new GeneratedEnemyUnit(-18,-15,96,39,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "fbecdce61da049d218f43941858dc66b777c672eb93926f27f6445ce5348e207");
        }

        private static void Case_03846()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3846,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,5,31,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-9,17,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,6,61,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-4,29,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,13,19,49,1), new GeneratedEnemyUnit(-14,5,55,46,2), new GeneratedEnemyUnit(18,4,37,1,4), new GeneratedEnemyUnit(-14,-20,80,42,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "7c72bb7bbba23f6245cd4ea077ee61a512a63f5d1a6d9120b345687b9667e4d3");
        }

        private static void Case_03847()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3847,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-3,20,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-7,71,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,18,67,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-4,14,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-19,98,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,16,96,6,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,0,68,27,4), new GeneratedEnemyUnit(0,-15,14,11,3), new GeneratedEnemyUnit(12,5,35,39,1), new GeneratedEnemyUnit(19,14,82,45,2), new GeneratedEnemyUnit(-14,1,81,30,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "587c4b1bdee02a2444cc2c4c2ee3aa4ecb42b274b427f83f03d1103a5da5cfd0");
        }

        private static void Case_03848()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3848,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,7,15,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,2,51,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,6,45,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-16,9,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,3,79,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,16,97,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,19,69,12,4), new GeneratedEnemyUnit(1,-1,75,31,4), new GeneratedEnemyUnit(0,15,90,24,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "c8fc0ee6968bcdc972b75ccadccb66e722f44415760e215b9e4cd507fc6a9b47");
        }

        private static void Case_03849()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3849,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-7,84,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,14,57,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,10,42,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-9,28,3,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "a9ae3954c1ba6e2cb457193af5d27940fb05ccccf4a333b0deadd8a351087fb2");
        }

        private static void Case_03850()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3850,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-7,87,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,13,97,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,4,72,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,2,87,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-13,35,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-14,22,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-7,43,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-18,34,40,4), new GeneratedEnemyUnit(-15,-3,14,44,1), new GeneratedEnemyUnit(1,3,53,34,2), new GeneratedEnemyUnit(-6,-13,78,5,1), new GeneratedEnemyUnit(-16,-17,81,9,1), new GeneratedEnemyUnit(14,3,60,6,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "79e264521c16ae13f870ec4c2def9ba66bcbd9dfa352a46b409901e3875dfab1");
        }

        private static void Case_03851()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3851,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-1,37,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,13,7,37,1), new GeneratedEnemyUnit(-17,-2,70,28,4), new GeneratedEnemyUnit(5,19,29,27,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "4004003508a57c8ccca84a00a93e17d9d81b71948c069b9f0954fcf4c3d624ee");
        }

        private static void Case_03852()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3852,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-14,37,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-20,19,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,11,60,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-14,7,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-5,78,36,4), new GeneratedEnemyUnit(19,-13,24,21,4), new GeneratedEnemyUnit(-9,15,9,36,1), new GeneratedEnemyUnit(-4,11,63,6,1), new GeneratedEnemyUnit(-15,10,69,49,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "92e2cb7c1bf80e6b391e490b48ebd8ff03d8ddde91ada5b1fe594ed4b149190f");
        }

        private static void Case_03853()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3853,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,10,89,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,12,59,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,18,58,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,18,76,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-2,27,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,0,72,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,8,71,41,2), new GeneratedEnemyUnit(10,6,93,48,3), new GeneratedEnemyUnit(-4,-20,86,30,1), new GeneratedEnemyUnit(3,11,31,39,3), new GeneratedEnemyUnit(11,12,47,45,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "ce12f723816b01c233db7e9ea507fbadb995a402f7d6314f8b8f2b3c24095de8");
        }

        private static void Case_03854()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3854,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-8,73,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,18,98,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-14,49,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-16,62,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-5,53,48,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "ec18519e1f14b6cc9c6f1588608d3b74bbe03ea90606550384fcac055203cdb5");
        }

        private static void Case_03855()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3855,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-13,17,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-11,74,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-13,98,32,3), new GeneratedEnemyUnit(-20,2,22,8,4), new GeneratedEnemyUnit(-17,-8,67,39,4), new GeneratedEnemyUnit(4,-17,40,41,3), new GeneratedEnemyUnit(-13,18,39,5,1), new GeneratedEnemyUnit(-13,15,79,9,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "d9252a1b8c4177024786f7621d7134165e41898e8bf57419786be23e789c7a0d");
        }

        private static void Case_03856()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3856,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-5,51,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,3,8,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-13,57,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,4,83,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-9,23,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,9,5,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-1,70,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,2,68,6,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-15,37,15,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "e7950e867ef82294d664190944fe8b08efb2c33a0c242812ce11834687eb7fe2");
        }

        private static void Case_03857()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3857,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,18,90,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-18,40,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,13,17,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-5,14,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,13,53,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,10,92,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,2,98,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,1,52,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-6,44,22,2), new GeneratedEnemyUnit(-5,-8,81,24,1), new GeneratedEnemyUnit(-19,5,69,33,3), new GeneratedEnemyUnit(-16,20,49,31,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "ef7ff01c89f447c3962f6f2bfb9900522b3abe4203b9933e888b05cfb6a3a5f6");
        }

        private static void Case_03858()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3858,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-18,26,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-15,13,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,14,18,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-15,66,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-12,17,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,4,72,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,8,46,17,3), new GeneratedEnemyUnit(-16,-5,43,30,3), new GeneratedEnemyUnit(-17,-18,7,18,1), new GeneratedEnemyUnit(-17,5,46,11,1), new GeneratedEnemyUnit(8,-13,5,27,3), new GeneratedEnemyUnit(-5,-5,21,25,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 40,
                stableHash: "9e08d42ab7f0f860cdbcf3cd039b24f255179a923ea31cf14c00846659ad8954");
        }

        private static void Case_03859()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3859,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-18,39,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,10,18,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,18,38,6,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,7,8,44,3), new GeneratedEnemyUnit(5,5,90,16,2), new GeneratedEnemyUnit(-9,-6,77,25,4), new GeneratedEnemyUnit(7,-16,58,12,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "f259d27177a46621736ee2df0244fcea8abb27f84ad0e8eab434c10f9e044fcd");
        }

        private static void Case_03860()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3860,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-19,38,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-6,25,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-12,79,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,5,100,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-15,16,6,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "97011e11ff3e9f1ccfd069fa91ef3e207721d4e490f038dff3dce3974a8642c3");
        }

        private static void Case_03861()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3861,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,8,94,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-8,96,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-15,38,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,16,32,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-11,47,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-2,76,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,7,60,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-11,76,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,19,55,37,3), new GeneratedEnemyUnit(-1,-5,71,15,1), new GeneratedEnemyUnit(9,1,46,30,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "88ea9e7080d389eb47c600319970661b3c9a9e8ea2412c4e6089ec48ce8ddf1a");
        }

        private static void Case_03862()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3862,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,3,5,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-7,50,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-9,39,1,3), new GeneratedEnemyUnit(5,0,12,14,2), new GeneratedEnemyUnit(-17,17,34,17,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "54f4cdc4d25cb08cc7fcf8bea60dfc21f4faa963baa32ced70bbc14ce7620719");
        }

        private static void Case_03863()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3863,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,7,5,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-19,99,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,9,58,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,0,32,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,20,53,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-12,92,43,1), new GeneratedEnemyUnit(15,13,17,34,2), new GeneratedEnemyUnit(-19,-11,46,20,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "783ec3736abca976fed7729e1c3088dd4bc1b77e005ce11062f733fe069376ab");
        }

        private static void Case_03864()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3864,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,6,15,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-4,64,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,3,39,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-8,7,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-14,49,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,17,70,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-9,75,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-20,43,41,4), new GeneratedEnemyUnit(0,19,29,11,2), new GeneratedEnemyUnit(14,18,13,26,2), new GeneratedEnemyUnit(4,-4,19,18,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "e09d09dc1048a9b0968727888c2fe5f37bc64ba8ae86f5763ed2e0cb4ceb0ba4");
        }

        private static void Case_03865()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3865,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-2,73,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,17,85,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-3,19,2,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,6,56,41,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "97d95624783f99d73d59c54a818b1d4b2afa05a9c3431b94fbca4f969b6bdcd4");
        }

        private static void Case_03866()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3866,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,6,86,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-17,5,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,2,97,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,5,38,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-3,77,40,4), new GeneratedEnemyUnit(-16,-8,100,32,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "2bbbea2c9738071df740d4862ac9c58afbf41ce05634bcc8a46fb53cbc4a58cd");
        }

        private static void Case_03867()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3867,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,10,97,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-16,93,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-10,66,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,18,61,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-6,11,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,2,9,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,4,47,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,19,28,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,10,69,18,1), new GeneratedEnemyUnit(16,7,55,18,4), new GeneratedEnemyUnit(16,-1,23,50,2), new GeneratedEnemyUnit(5,-12,83,20,2), new GeneratedEnemyUnit(7,-19,78,41,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "65234013010c62d4dc4ec89bf6015accfec4d1e56a9c27926e3c6278addde92a");
        }

        private static void Case_03868()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3868,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,0,75,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,18,48,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-20,31,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-3,53,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,10,70,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-14,20,5,1), new GeneratedEnemyUnit(-4,-19,80,38,4), new GeneratedEnemyUnit(15,1,33,29,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "dfd94848d94fd22a1d6b411e3b8dfeb46df5846be818fef6a2cf392ce9563d81");
        }

        private static void Case_03869()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3869,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-4,56,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,2,20,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-15,68,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-16,96,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-16,94,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,1,6,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,4,59,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,15,54,50,3), new GeneratedEnemyUnit(3,2,27,32,4), new GeneratedEnemyUnit(-11,-2,58,50,4), new GeneratedEnemyUnit(0,11,41,12,1), new GeneratedEnemyUnit(-19,16,83,10,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "6e1381228db04dc952703e9e449abea332518c6edc01a856b9c3390d86cba539");
        }

        private static void Case_03870()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3870,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,6,53,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-2,67,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-19,79,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-1,47,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-19,22,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,19,18,3,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "d165f258ababd58a1b5c6ddc4b071a50e2131af6e2243930bd03c3b58bcffc07");
        }

        private static void Case_03871()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3871,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-16,98,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-8,71,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,4,64,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,8,89,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-10,17,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-9,21,8,4), new GeneratedEnemyUnit(4,-7,44,8,3), new GeneratedEnemyUnit(-2,4,45,13,4), new GeneratedEnemyUnit(5,20,67,19,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "262ad367dff723085f63940734f78fac763d0ac6263bfca08c18285640a3b048");
        }

        private static void Case_03872()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3872,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,12,30,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,6,25,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-12,94,8,4), new GeneratedEnemyUnit(2,-7,63,6,2), new GeneratedEnemyUnit(1,-10,28,37,2), new GeneratedEnemyUnit(-11,15,9,8,3), new GeneratedEnemyUnit(9,1,30,18,4), new GeneratedEnemyUnit(-18,9,33,45,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "da73363b811fed2416bb68d0b8a1a7c82311e81d4fc272dff4dbe1911a8b7e64");
        }

        private static void Case_03873()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3873,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-15,19,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-1,31,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,0,58,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,15,16,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,8,57,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,18,60,15,1), new GeneratedEnemyUnit(11,-5,24,23,2), new GeneratedEnemyUnit(-5,-3,9,35,3), new GeneratedEnemyUnit(18,-19,86,43,2), new GeneratedEnemyUnit(4,-5,29,9,3), new GeneratedEnemyUnit(18,18,27,7,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "21bbbc623f7418e1a3a6a7f2de24378c2c7ab811b33c53a5873566a7b0ce7803");
        }

        private static void Case_03874()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3874,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-15,48,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-3,48,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,15,62,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-3,5,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,15,32,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-1,31,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,20,13,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-1,41,17,2), new GeneratedEnemyUnit(-5,-13,82,1,3), new GeneratedEnemyUnit(17,1,70,30,3), new GeneratedEnemyUnit(10,-9,37,34,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "1843b063efb7b186c928123d9981ab6336fdcc234693f67a581f799fee0f7d0a");
        }

        private static void Case_03875()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3875,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,5,95,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,8,92,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,1,23,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-19,51,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,13,56,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-8,23,17,1), new GeneratedEnemyUnit(-19,-18,19,39,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "41cad22edb659b30bcd7860e02db5a6d182b3f556bb5c1f396862e0d07cccdfb");
        }

        private static void Case_03876()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3876,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,2,66,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,7,45,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,10,71,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-18,47,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-11,66,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,11,55,7,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,8,9,26,4), new GeneratedEnemyUnit(-4,19,87,34,2), new GeneratedEnemyUnit(-7,12,70,28,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "a3b9c1ac121157b95ff2d79d552252e77bacb7aaa7c632fd8e6643959a16a9bc");
        }

        private static void Case_03877()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3877,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,6,63,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-13,38,32,2), new GeneratedEnemyUnit(15,-6,7,47,2), new GeneratedEnemyUnit(-5,-20,87,18,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "efaedd0f5debcd1818f4fb7e0ec31f7a8700bde14878ecb7e8da1e2f98055689");
        }

        private static void Case_03878()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3878,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-17,80,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,4,10,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,9,63,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-14,93,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,8,73,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-2,14,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,11,44,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,7,29,15,2), new GeneratedEnemyUnit(-1,20,44,43,4), new GeneratedEnemyUnit(-19,-6,5,27,1), new GeneratedEnemyUnit(-4,-16,5,9,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "3fbfdc52f953705b0a72a98582e60532d8f186dcdee9d22fa0118de8f867588d");
        }

        private static void Case_03879()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3879,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,13,18,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,11,63,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-12,87,31,2), new GeneratedEnemyUnit(-1,-1,98,15,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "13cd3b8ebf710687c71d1923b2d8402f05ca0c392bdd858de6b903579b9904a1");
        }

        private static void Case_03880()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3880,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-14,55,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-10,55,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,7,99,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-20,26,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-14,42,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,7,65,19,2), new GeneratedEnemyUnit(-1,-5,29,40,4), new GeneratedEnemyUnit(-1,14,17,48,4), new GeneratedEnemyUnit(10,-13,71,23,2), new GeneratedEnemyUnit(2,-3,46,33,2), new GeneratedEnemyUnit(0,-20,7,36,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "7b4a60a867f3e14f43cee089d5b915e0878cd7217094f606f920ea21cd2a3568");
        }

        private static void Case_03881()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3881,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-11,99,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-20,27,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,16,47,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-1,65,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,7,94,32,1), new GeneratedEnemyUnit(11,-18,49,39,3), new GeneratedEnemyUnit(-16,13,41,39,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "56ddd0f422fe7883fd97afb5a2933845dfe3427472404d3ace06a09e4561d5bc");
        }

        private static void Case_03882()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3882,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,7,80,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-14,59,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-12,48,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,9,68,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-12,39,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-16,33,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,6,53,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,16,62,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-11,94,42,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 20,
                stableHash: "81a901c31eeb546a7a5f08d2e9a22a9b13bd2def713430b6d571cb5df5faf91f");
        }

        private static void Case_03883()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3883,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-3,17,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-2,62,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,6,40,6,3), new GeneratedEnemyUnit(4,1,56,49,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 25,
                stableHash: "006bb14de2cc6cbb75652b87e011b8f30de6d930fa7117c64886e6a56bddab11");
        }

        private static void Case_03884()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3884,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-9,59,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-18,83,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-8,84,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,20,87,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-2,34,46,4), new GeneratedEnemyUnit(9,-14,5,21,4), new GeneratedEnemyUnit(16,1,100,42,2), new GeneratedEnemyUnit(-4,3,47,33,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "664bfa034b114a8e0c5564bef231014cd1dd8db24402b6bc7e84ab38787a38ff");
        }

        private static void Case_03885()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3885,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-16,60,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,18,37,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-8,62,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,7,46,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,10,46,49,4), new GeneratedEnemyUnit(-7,-11,93,24,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "af5f311266b5e0d20d31a2294122f6bf65e0491956fb77a1739ed14a508fe23d");
        }

        private static void Case_03886()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3886,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,19,50,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,8,28,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,10,79,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-3,71,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-8,93,25,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "44ea76ee43074f93a05f8b29f04271bbea24bdf1d5988593996819221de16627");
        }

        private static void Case_03887()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3887,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-12,65,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,0,12,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,5,68,3,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "cb7598e3c02ad12bccd686ca903bcd6dd9e64e139a7b7f9ea7775a8d522de4c1");
        }

        private static void Case_03888()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3888,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-12,65,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-4,21,21,1), new GeneratedEnemyUnit(14,14,64,33,2), new GeneratedEnemyUnit(3,-11,47,45,2), new GeneratedEnemyUnit(-2,13,100,12,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "3092f5f9fa5e851b231e10c79d1b87cccbefe802fb56242e3d9d54734a7ba427");
        }

        private static void Case_03889()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3889,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,18,95,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-17,32,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-1,12,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,6,20,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-8,44,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,3,51,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,1,28,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-20,76,28,3), new GeneratedEnemyUnit(-17,10,59,46,1), new GeneratedEnemyUnit(8,-3,50,10,4), new GeneratedEnemyUnit(-7,10,13,13,3), new GeneratedEnemyUnit(-11,16,57,4,1), new GeneratedEnemyUnit(17,19,79,14,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "4db5c8021bb099e4227f3f2b59193813bfd92402aa3884bc91e3e667f624606f");
        }

        private static void Case_03890()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3890,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,4,68,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-7,76,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,18,63,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-9,74,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,11,29,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-7,75,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-11,61,35,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "6f1026699f546bddf6abe9b58dff885319b5e87e479deb5a66d6ab8c567c7078");
        }

        private static void Case_03891()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3891,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-13,15,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-17,7,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,13,54,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,8,97,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-1,7,27,3), new GeneratedEnemyUnit(-6,17,43,32,1), new GeneratedEnemyUnit(7,-14,34,40,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "d5cbb9bf129b85e2032cc4b3467716bdbb2dddfeef43221ef95c9f3a183e1b40");
        }

        private static void Case_03892()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3892,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,11,16,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,8,16,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-9,99,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,1,59,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,16,61,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,9,33,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-2,28,27,4), new GeneratedEnemyUnit(-19,15,19,5,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "1de3ecbe60ee574ead43b432f31ad7dd15b6cf9d7882246cfe564cad95056c96");
        }

        private static void Case_03893()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3893,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-1,19,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-15,87,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,12,87,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-6,6,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,4,30,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,4,93,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,3,28,42,3), new GeneratedEnemyUnit(9,-16,93,19,4), new GeneratedEnemyUnit(3,-13,5,15,1), new GeneratedEnemyUnit(3,10,20,30,2), new GeneratedEnemyUnit(-19,18,89,10,3), new GeneratedEnemyUnit(-1,20,58,37,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "0ace798e2c17762113c4dc9970fd2f81a164920a840de8e69c8b318249219446");
        }

        private static void Case_03894()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3894,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,20,51,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,9,39,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,14,97,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,13,92,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,15,35,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,17,71,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,4,40,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-6,92,26,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "d855d4fac4f9691c75b2e1eeccf15f7d3a4bef9984f88b635da877cac79393d4");
        }

        private static void Case_03895()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3895,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,16,54,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-19,100,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,2,48,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-10,35,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,20,41,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,3,21,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,8,94,10,4), new GeneratedEnemyUnit(15,-3,76,32,3), new GeneratedEnemyUnit(1,-6,100,7,1), new GeneratedEnemyUnit(-2,-1,48,23,1), new GeneratedEnemyUnit(-17,0,96,32,4), new GeneratedEnemyUnit(-14,14,61,14,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "311f42659e16ba2a6b2754832566b08cda748d3205533a231d9ae945ec88e000");
        }

        private static void Case_03896()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3896,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,19,95,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-9,88,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,8,77,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-17,48,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-13,49,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,17,5,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,18,96,48,1), new GeneratedEnemyUnit(5,-6,51,36,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "5b2e06de94e4ae604abd5838769ceac7bd4f9d08a4cb1f5446512eaa2080d0a8");
        }

        private static void Case_03897()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3897,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,1,88,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,15,48,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-1,6,2,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "07aaf784022adc8268205a6207589d052563c1c1fd4337365f0ce7ab47d756f4");
        }

        private static void Case_03898()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3898,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,17,73,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,14,13,25,1), new GeneratedEnemyUnit(19,-5,56,5,1), new GeneratedEnemyUnit(-8,-11,74,40,1), new GeneratedEnemyUnit(-2,-17,13,28,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "60dce483cae00ab91182f2b9f3d25aa604b58360022cf8bf766ced43de47560f");
        }

        private static void Case_03899()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3899,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,16,70,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,3,50,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-3,74,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-4,63,24,1), new GeneratedEnemyUnit(-4,5,51,26,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "584c4118367d18293754a24414be2c280ad7f04f0f2fe076656190101cf4c891");
        }

        private static void Case_03900()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3900,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,19,59,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-18,70,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-12,94,4,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "1d8aeabdebdfe662a688f9bfa5100b07e473c0ed7884c25b10b2f609de774186");
        }

        private static void Case_03901()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3901,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-10,88,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,7,86,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-7,80,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,18,77,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-4,54,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,0,75,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-3,70,4,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "e19c31a213cd432f5e27e7f0c5ce119c9d27cae80b2195f1679f4059d7eaecae");
        }

        private static void Case_03902()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3902,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,20,58,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,11,6,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,11,85,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,9,25,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,7,60,26,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "6fa981d100cc707653917e2ec2b56b5cffd2ed8200a50afa48e955b3a3441822");
        }

        private static void Case_03903()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3903,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,2,18,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,5,11,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,10,29,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,14,51,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-18,29,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-17,65,45,1), new GeneratedEnemyUnit(14,-10,42,35,4), new GeneratedEnemyUnit(-20,4,49,21,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "715e6e168628fd0962aeb453320adea52f690f5dd285e6af6d42c2991b6f09b1");
        }

        private static void Case_03904()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3904,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,2,81,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,9,30,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,0,57,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,4,16,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,12,80,20,1), new GeneratedEnemyUnit(-15,-12,52,25,4), new GeneratedEnemyUnit(13,9,27,12,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "604eada3ce4e063c5ef787b6cd91ea1eedec830f149166c0a0e87989b3c6164a");
        }

        private static void Case_03905()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3905,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,13,45,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-9,41,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-19,58,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-8,44,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-14,97,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,13,93,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-16,93,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,13,64,3,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "b269de6093a1a4d693a8e50efc8f49e17733df31dd6185a1953877f0ea0289e7");
        }

        private static void Case_03906()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3906,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,11,17,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-9,90,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-6,59,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-2,39,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-15,10,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,19,98,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,4,40,21,2), new GeneratedEnemyUnit(-16,7,69,13,2), new GeneratedEnemyUnit(14,-11,77,21,2), new GeneratedEnemyUnit(13,18,5,7,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "bc128815e0b96742b5486414a4ffccf3151973700be91a3f99e08f869fe36450");
        }

        private static void Case_03907()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3907,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,16,25,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-7,24,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,13,87,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,10,40,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,20,42,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-8,74,18,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "7434506877b8d322725856bfc195b9fb3c67a6ced085a6f48da95d7a9b428124");
        }

        private static void Case_03908()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3908,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-5,12,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-16,5,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-18,27,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-7,80,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-16,17,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-9,98,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-16,57,44,4), new GeneratedEnemyUnit(14,15,55,12,3), new GeneratedEnemyUnit(-10,-8,12,18,4), new GeneratedEnemyUnit(3,-16,68,19,1), new GeneratedEnemyUnit(15,17,90,37,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "3216c52c51859f10279f48e55288322f56612f943e9e78014c9a3543f231ca63");
        }

        private static void Case_03909()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3909,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-7,20,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-7,98,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,12,87,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-9,53,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,16,6,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-19,18,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-2,67,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,3,57,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-2,57,41,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 75,
                stableHash: "2145a49699adacc8d28430206f3359915d80f6040527d134b574fa19e26cc0ef");
        }

        private static void Case_03910()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3910,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-14,100,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,20,31,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-6,47,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-1,43,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-17,34,35,4), new GeneratedEnemyUnit(-4,8,74,23,1), new GeneratedEnemyUnit(-9,17,15,1,2), new GeneratedEnemyUnit(-9,20,11,43,2), new GeneratedEnemyUnit(-8,4,11,5,1), new GeneratedEnemyUnit(-13,8,35,15,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "83e5b06d9249039bf3c0602cc7ac95dc6d9f56feaa0c65f4d6857877c770c519");
        }

        private static void Case_03911()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3911,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-16,55,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-9,26,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-3,67,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,16,47,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,0,86,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,6,97,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,7,35,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,3,12,1,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "1782d610fe1734bc77651b4f8877fb6ce236c5fd6d5809518b7702279dea8e4b");
        }

        private static void Case_03912()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3912,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-6,14,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,15,83,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,3,60,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,11,26,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-18,22,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-3,65,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,7,97,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,15,84,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-8,73,5,4), new GeneratedEnemyUnit(5,0,66,37,3), new GeneratedEnemyUnit(-3,2,23,38,4), new GeneratedEnemyUnit(12,-4,21,50,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "7eef67a06e32271a49f2c5cf77529bdc1a25fcb8f67c8929cf222865a4bdef0e");
        }

        private static void Case_03913()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3913,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-5,31,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,8,5,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,13,26,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,4,67,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-14,99,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,1,41,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,8,60,2,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,14,76,36,3), new GeneratedEnemyUnit(-17,-7,67,13,2), new GeneratedEnemyUnit(7,18,52,36,2), new GeneratedEnemyUnit(13,-16,24,1,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "f07db8d7af1f52687688d7e67b82734300e594847d83e765c2502ad58c2f392f");
        }

        private static void Case_03914()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3914,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,6,82,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,11,44,7,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "9a93434c1ad12b24efe5552670e31657fed05b978250605e25466198aa71baa5");
        }

        private static void Case_03915()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3915,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,9,92,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-10,92,34,4), new GeneratedEnemyUnit(13,0,37,43,4), new GeneratedEnemyUnit(-9,-4,24,24,1), new GeneratedEnemyUnit(6,8,5,12,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "0a4639b908f8c5635fff491257c05014928625720e257e983cf48690fda1eef4");
        }

        private static void Case_03916()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3916,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,16,56,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,0,40,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-8,56,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,7,35,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-14,46,5,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "fcef59134c3d59cb9b0d673dbb425ee1ea8ce46483f9a6c0246e8cbf8d84f55f");
        }

        private static void Case_03917()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3917,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,3,96,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-4,30,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-3,80,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-10,7,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,3,33,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-15,15,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-18,83,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,1,80,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,11,50,39,1), new GeneratedEnemyUnit(4,-19,96,1,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "ed3f1a558b4555b25b760c6e1423f3952e665565017aba1afc192c89ff3b6fa4");
        }

        private static void Case_03918()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3918,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-9,54,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-5,18,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,18,26,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-3,59,11,4), new GeneratedEnemyUnit(1,4,86,14,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "1276f87cad6ddd38fa4bedde0171f08a13443c7fec0f23a3f0f9e06fe29bea75");
        }

        private static void Case_03919()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3919,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-19,99,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,9,96,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-18,65,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,4,45,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,17,90,2,3), new GeneratedEnemyUnit(6,4,84,36,2), new GeneratedEnemyUnit(0,-8,42,46,2), new GeneratedEnemyUnit(-10,8,78,39,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "e25499b6d7c6eefb428a9546365f9a3b282494bccfa89583636acaa62d382adb");
        }

        private static void Case_03920()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3920,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-6,14,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-1,71,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,2,15,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-11,72,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,9,28,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,15,94,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-18,11,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,7,51,40,4), new GeneratedEnemyUnit(20,-1,99,38,1), new GeneratedEnemyUnit(-20,-4,69,39,1), new GeneratedEnemyUnit(6,-18,47,43,1), new GeneratedEnemyUnit(-7,8,30,26,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "596feb44f6e7e141c952280c462d8fbd6d553cbe3962ff114194c9cd8ffc0281");
        }

        private static void Case_03921()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3921,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,1,50,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-7,60,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-18,32,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,13,52,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,9,24,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-3,35,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,0,42,1,4), new GeneratedEnemyUnit(19,17,31,24,4), new GeneratedEnemyUnit(-6,-15,96,1,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "de6772e26c992d77c00b253da6a020325c832edca8e45ea11d914e6204fb629d");
        }

        private static void Case_03922()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3922,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,10,72,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,13,68,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,11,36,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,20,57,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-18,73,6,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "b51c41047c1ac7e401abab5673ef81cda4eb9b2f6d974109dee643ce53b59324");
        }

        private static void Case_03923()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3923,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-6,51,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-14,62,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,14,55,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,8,48,18,1), new GeneratedEnemyUnit(4,-9,85,25,3), new GeneratedEnemyUnit(1,-15,42,22,4), new GeneratedEnemyUnit(-5,4,98,33,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "e07e15797a036255a08e8a2dc5ddfbe114cd4f9607bd2b7d8406fbeb20056394");
        }

        private static void Case_03924()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3924,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,4,28,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-20,46,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,15,7,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-12,62,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,16,26,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,18,95,21,1), new GeneratedEnemyUnit(5,-12,98,47,3), new GeneratedEnemyUnit(17,-19,58,39,4), new GeneratedEnemyUnit(-1,16,87,42,1), new GeneratedEnemyUnit(0,14,23,30,1), new GeneratedEnemyUnit(-9,-19,89,3,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "c54a2f65decd51b058a60a5b99ce447e8e02ac99e096cf397522ac8eaae2bc74");
        }

        private static void Case_03925()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3925,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,19,45,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-2,62,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,16,83,36,2), new GeneratedEnemyUnit(-9,20,64,11,2), new GeneratedEnemyUnit(-2,9,14,14,4), new GeneratedEnemyUnit(7,19,89,45,3), new GeneratedEnemyUnit(1,12,37,19,4), new GeneratedEnemyUnit(-9,8,9,14,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "f52aa1aad1f7445af164199747d641e23ec42ad2c0d54fd1bc8387691748bd64");
        }

        private static void Case_03926()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3926,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,2,61,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-2,19,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,20,79,46,1), new GeneratedEnemyUnit(17,-7,79,40,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "75e69dd7e595d8eb03a859ed7bf661c0e2bd7f8d45faafe3531641c835b623f4");
        }

        private static void Case_03927()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3927,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-4,77,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-10,86,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-7,19,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,13,48,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,12,9,1,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "21586875e1d9d5956a2faf28b1d32d6f208e339e4a7fb226c42061fed152d8cc");
        }

        private static void Case_03928()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3928,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,18,65,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-10,29,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-19,27,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-13,21,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-9,71,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,10,66,3,2), new GeneratedEnemyUnit(-20,-9,67,32,2), new GeneratedEnemyUnit(-7,-3,94,33,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "b8f47838d866bce4df633f34aef7764b989a765329c7b2d988689fcaea612de1");
        }

        private static void Case_03929()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3929,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-18,59,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-2,88,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,4,65,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,9,69,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-7,40,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-1,99,5,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "e50bc6d3bd99dc93e0d7e1447b19364c43071a3df947ae4ee498b64e7debff8a");
        }

        private static void Case_03930()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3930,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,20,84,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-4,65,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,17,64,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-10,96,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-2,15,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,10,81,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,7,21,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-19,46,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,6,11,36,2), new GeneratedEnemyUnit(-3,13,18,43,4), new GeneratedEnemyUnit(-19,12,26,34,1), new GeneratedEnemyUnit(19,6,98,38,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "28c2ede7e3ad71b8dd0960497b016ea116ff1012fa5f9c5ac027749c4c0d66b1");
        }

        private static void Case_03931()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3931,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-15,18,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,11,99,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-10,51,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-3,86,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,7,7,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-4,65,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-13,69,49,1), new GeneratedEnemyUnit(-2,-18,60,14,2), new GeneratedEnemyUnit(-20,14,92,49,2), new GeneratedEnemyUnit(19,12,98,25,3), new GeneratedEnemyUnit(-16,3,38,36,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "59897465af002366e0eae5713f08bccda15cd93f50945136041cda6ac42153aa");
        }

        private static void Case_03932()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3932,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,7,80,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,8,98,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,17,65,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,13,80,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,3,28,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,15,57,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-16,93,17,1), new GeneratedEnemyUnit(-8,3,13,22,1), new GeneratedEnemyUnit(-10,-3,62,2,4), new GeneratedEnemyUnit(8,12,64,6,2), new GeneratedEnemyUnit(-4,20,36,11,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "5ce39689ce56b93ae297634d00c95342a8e1a88a05627c2ddb9c9e5b203464b7");
        }

        private static void Case_03933()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3933,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-3,76,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,20,60,19,4), new GeneratedEnemyUnit(9,-11,29,22,3), new GeneratedEnemyUnit(-17,19,99,12,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "a463cbaed6ed90a6b4df928518e51b2b66afbcb7369ee772e8f3eb71522e62ce");
        }

        private static void Case_03934()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3934,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-9,98,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,18,36,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,10,76,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-5,60,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,5,88,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,2,68,3,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "34c7e7f1a77f659ffdfa85ee2733987a763460eb6529843139fb814e029a29b2");
        }

        private static void Case_03935()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3935,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,18,19,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-8,52,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,1,90,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,15,21,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,8,94,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-4,8,21,2), new GeneratedEnemyUnit(2,-4,25,29,4), new GeneratedEnemyUnit(16,-5,32,16,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "5a402a53016cbf8897aa7038681de34fe4b153975826d142ad54c654d5b28a9e");
        }

        private static void Case_03936()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3936,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-8,20,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,2,79,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,12,61,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-8,46,20,2), new GeneratedEnemyUnit(19,-17,49,6,2), new GeneratedEnemyUnit(-1,-3,47,5,4), new GeneratedEnemyUnit(3,17,15,19,3), new GeneratedEnemyUnit(13,-14,10,43,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 55,
                stableHash: "48b9bd225923e49158e1485fb1e53048ecc55700b2a7a66e384c6ac110cec89a");
        }

        private static void Case_03937()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3937,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,5,24,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,8,60,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-14,75,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-20,21,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-11,84,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-2,56,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-11,17,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-9,88,13,4), new GeneratedEnemyUnit(10,-12,38,10,1), new GeneratedEnemyUnit(-9,-16,5,25,3), new GeneratedEnemyUnit(17,-11,89,43,4), new GeneratedEnemyUnit(-19,-18,8,2,4), new GeneratedEnemyUnit(10,20,40,7,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "a6c908f46bb0088f9f719a37cbec1f06d35dee2dfeb59813e5c1bf8df98aa29b");
        }

        private static void Case_03938()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3938,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,12,19,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,3,98,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,5,17,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-12,10,49,1), new GeneratedEnemyUnit(13,-1,34,41,2), new GeneratedEnemyUnit(-15,-13,8,26,4), new GeneratedEnemyUnit(13,-15,87,35,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "aaf776101abf6f24d8846302557a777af1415603882eb602c9cba2b4952ea6a7");
        }

        private static void Case_03939()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3939,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-7,56,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,5,98,48,4), new GeneratedEnemyUnit(-2,5,46,25,1), new GeneratedEnemyUnit(17,18,33,44,3), new GeneratedEnemyUnit(-20,7,100,38,4), new GeneratedEnemyUnit(1,-20,87,25,4), new GeneratedEnemyUnit(16,-4,45,25,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "0edb3cb72b640d35799675848f8906aba441eb9591c6c4938c825959fbed2c0e");
        }

        private static void Case_03940()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3940,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,6,86,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,17,40,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,16,29,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-16,6,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,19,72,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-11,93,37,1), new GeneratedEnemyUnit(18,13,57,3,1), new GeneratedEnemyUnit(-6,20,62,4,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "080c9ae0b9a318f206edd08cc35e2345ce98c8edeefdc7fd0b19ec081803b883");
        }

        private static void Case_03941()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3941,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,20,59,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-8,98,5,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 15,
                stableHash: "ac8aecc00cad7273732365c628e92687fd04af84529b4cfeeb9efb4c7ae8b863");
        }

        private static void Case_03942()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3942,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-15,28,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,5,24,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,0,51,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-19,18,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,16,17,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,19,21,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-16,47,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-4,79,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,4,10,26,2), new GeneratedEnemyUnit(13,13,9,19,1), new GeneratedEnemyUnit(1,-20,58,37,2), new GeneratedEnemyUnit(-11,18,73,9,4), new GeneratedEnemyUnit(2,-1,91,16,2), new GeneratedEnemyUnit(13,-14,64,3,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "d8a22c2e42ad35fbb1bc8cc4dee3bd15d02f83a58b22a0b4f60ab35305828949");
        }

        private static void Case_03943()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3943,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-1,25,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,14,21,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,17,60,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,16,67,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,12,80,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,13,14,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,5,44,32,1), new GeneratedEnemyUnit(18,20,46,47,4), new GeneratedEnemyUnit(-9,-11,6,2,2), new GeneratedEnemyUnit(4,18,87,27,3), new GeneratedEnemyUnit(-10,0,49,37,4), new GeneratedEnemyUnit(-4,17,82,30,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "acadf3aa2747bc870d2d2221e1bfafe98dd79b70dba2e6c6b0bb0ccbba05e8a3");
        }

        private static void Case_03944()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3944,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,20,66,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-14,73,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,4,78,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-8,100,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-16,74,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,17,33,3,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "4a883743b47006981a9ac316aeffe3459a9403793fab62dd80da8533063607c7");
        }

        private static void Case_03945()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3945,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,20,33,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-3,74,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,8,17,49,4), new GeneratedEnemyUnit(4,0,16,41,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "130c9bd44109153b2d7f339a334951d22576ce1ba23ae831bc459432a90934f2");
        }

        private static void Case_03946()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3946,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,14,20,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-19,17,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,9,20,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,5,33,4,2), new GeneratedEnemyUnit(7,18,80,39,4), new GeneratedEnemyUnit(-1,-19,22,25,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "57b2b48439ffb11911c1f4792c57a898f5bf552b5f07ec061da0451d9a290b67");
        }

        private static void Case_03947()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3947,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-14,59,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,3,43,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-6,8,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,8,70,29,3), new GeneratedEnemyUnit(15,-14,18,19,4), new GeneratedEnemyUnit(12,1,34,49,3), new GeneratedEnemyUnit(-11,-4,8,27,4), new GeneratedEnemyUnit(17,12,33,11,1), new GeneratedEnemyUnit(-12,2,84,47,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "fe032f55c1d1b892767d496253690ead5212b87259d11fa8dbcfaac36a203436");
        }

        private static void Case_03948()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3948,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,7,61,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-9,78,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-17,49,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-14,66,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,2,75,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,5,8,26,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "a0e9aefaac68e4bb02bf45b09645133530a5348ee8a1bb33a463e2812e78d481");
        }

        private static void Case_03949()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3949,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-4,62,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-9,67,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,2,93,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,17,29,11,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "b11b3b7e74c52254f6484b4cf925c0414aa2a3e5fa3401d4966eec41a2e58fe7");
        }

        private static void Case_03950()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3950,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-8,49,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-7,58,6,3), new GeneratedEnemyUnit(-5,19,47,43,2), new GeneratedEnemyUnit(13,5,70,26,1), new GeneratedEnemyUnit(12,-9,27,17,2), new GeneratedEnemyUnit(13,-5,6,3,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "1142c17b21b1ca6c8cb0f4243664da8b7b7f630284c4c481e07aa1f7393e49a4");
        }

        private static void Case_03951()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3951,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-8,40,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,15,29,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,13,73,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-6,26,4,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "44c6878f652c96ef94f193b2704c6d1f74fe8efddee62745189d95209735eb44");
        }

        private static void Case_03952()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3952,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-12,22,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-2,43,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-18,51,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-20,13,16,2), new GeneratedEnemyUnit(19,-5,61,15,3), new GeneratedEnemyUnit(-14,-12,18,26,2), new GeneratedEnemyUnit(3,-5,36,29,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "90b6891cf18598a065d9dbc4c15f786ec87aa0bcb34407684f30fbbabdd17ed9");
        }

        private static void Case_03953()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3953,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,15,52,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,4,70,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-9,77,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,12,25,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-19,47,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,3,56,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-12,45,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,12,7,4,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,7,55,1,1), new GeneratedEnemyUnit(-19,5,36,37,4), new GeneratedEnemyUnit(-6,-4,19,19,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "ae1784765cce0d3778db2422f4a122e27fd2c55e7fcff255ac90ecd833cbb486");
        }

        private static void Case_03954()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3954,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-13,59,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,11,57,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-11,86,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,6,40,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-3,98,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-20,61,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-16,16,44,2), new GeneratedEnemyUnit(-5,11,46,1,1), new GeneratedEnemyUnit(-4,7,98,27,4), new GeneratedEnemyUnit(-12,-10,82,18,4), new GeneratedEnemyUnit(2,8,70,40,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "d9a5ae17fc572999877cd9255dd50015f5aa8853efad4c8bb40856215f5cd6f5");
        }

        private static void Case_03955()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3955,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-8,92,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,3,66,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-19,21,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,1,57,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-16,10,25,4), new GeneratedEnemyUnit(17,-10,79,49,1), new GeneratedEnemyUnit(-6,-14,64,33,4), new GeneratedEnemyUnit(5,6,90,14,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "f19678fe065e4031099bfbfc14da72371889e90a5a55846ce27972684d79b9ca");
        }

        private static void Case_03956()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3956,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-3,63,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-14,21,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,0,100,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,1,47,9,3), new GeneratedEnemyUnit(-17,-9,71,27,4), new GeneratedEnemyUnit(-13,4,74,6,2), new GeneratedEnemyUnit(-2,-12,64,35,1), new GeneratedEnemyUnit(-17,-1,100,14,1), new GeneratedEnemyUnit(13,18,66,16,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "ebff960d9027dafef98fa724ebd441243c6d9fb827f61674eee41cc7dae14661");
        }

        private static void Case_03957()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3957,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,13,24,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,16,51,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,11,22,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-14,82,43,3), new GeneratedEnemyUnit(17,5,99,3,4), new GeneratedEnemyUnit(-12,-7,93,34,1), new GeneratedEnemyUnit(-12,10,10,2,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "802c84ac58b405da796abec56e881c7515be4b0a0192a3aabe386a4d1ec48584");
        }

        private static void Case_03958()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3958,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,13,30,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-9,96,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,15,17,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-5,70,34,4), new GeneratedEnemyUnit(-13,10,47,33,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "58e11b5eee84512728cd5a7b38b7d877ef864e9b49e2ed574f14cc31b50ddcd9");
        }

        private static void Case_03959()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3959,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-12,45,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-6,93,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,10,58,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-16,9,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,10,68,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-8,77,40,1), new GeneratedEnemyUnit(-3,13,46,12,4), new GeneratedEnemyUnit(-19,16,54,41,1), new GeneratedEnemyUnit(13,4,60,32,2), new GeneratedEnemyUnit(18,-13,86,23,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "b8b0b2b4c490eca51a2c5ea62455b284673402158b2cf0d0587882ffd8aa7c76");
        }

    }
}
