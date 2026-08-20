using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard010
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_01800();
            Case_01801();
            Case_01802();
            Case_01803();
            Case_01804();
            Case_01805();
            Case_01806();
            Case_01807();
            Case_01808();
            Case_01809();
            Case_01810();
            Case_01811();
            Case_01812();
            Case_01813();
            Case_01814();
            Case_01815();
            Case_01816();
            Case_01817();
            Case_01818();
            Case_01819();
            Case_01820();
            Case_01821();
            Case_01822();
            Case_01823();
            Case_01824();
            Case_01825();
            Case_01826();
            Case_01827();
            Case_01828();
            Case_01829();
            Case_01830();
            Case_01831();
            Case_01832();
            Case_01833();
            Case_01834();
            Case_01835();
            Case_01836();
            Case_01837();
            Case_01838();
            Case_01839();
            Case_01840();
            Case_01841();
            Case_01842();
            Case_01843();
            Case_01844();
            Case_01845();
            Case_01846();
            Case_01847();
            Case_01848();
            Case_01849();
            Case_01850();
            Case_01851();
            Case_01852();
            Case_01853();
            Case_01854();
            Case_01855();
            Case_01856();
            Case_01857();
            Case_01858();
            Case_01859();
            Case_01860();
            Case_01861();
            Case_01862();
            Case_01863();
            Case_01864();
            Case_01865();
            Case_01866();
            Case_01867();
            Case_01868();
            Case_01869();
            Case_01870();
            Case_01871();
            Case_01872();
            Case_01873();
            Case_01874();
            Case_01875();
            Case_01876();
            Case_01877();
            Case_01878();
            Case_01879();
            Case_01880();
            Case_01881();
            Case_01882();
            Case_01883();
            Case_01884();
            Case_01885();
            Case_01886();
            Case_01887();
            Case_01888();
            Case_01889();
            Case_01890();
            Case_01891();
            Case_01892();
            Case_01893();
            Case_01894();
            Case_01895();
            Case_01896();
            Case_01897();
            Case_01898();
            Case_01899();
            Case_01900();
            Case_01901();
            Case_01902();
            Case_01903();
            Case_01904();
            Case_01905();
            Case_01906();
            Case_01907();
            Case_01908();
            Case_01909();
            Case_01910();
            Case_01911();
            Case_01912();
            Case_01913();
            Case_01914();
            Case_01915();
            Case_01916();
            Case_01917();
            Case_01918();
            Case_01919();
            Case_01920();
            Case_01921();
            Case_01922();
            Case_01923();
            Case_01924();
            Case_01925();
            Case_01926();
            Case_01927();
            Case_01928();
            Case_01929();
            Case_01930();
            Case_01931();
            Case_01932();
            Case_01933();
            Case_01934();
            Case_01935();
            Case_01936();
            Case_01937();
            Case_01938();
            Case_01939();
            Case_01940();
            Case_01941();
            Case_01942();
            Case_01943();
            Case_01944();
            Case_01945();
            Case_01946();
            Case_01947();
            Case_01948();
            Case_01949();
            Case_01950();
            Case_01951();
            Case_01952();
            Case_01953();
            Case_01954();
            Case_01955();
            Case_01956();
            Case_01957();
            Case_01958();
            Case_01959();
            Case_01960();
            Case_01961();
            Case_01962();
            Case_01963();
            Case_01964();
            Case_01965();
            Case_01966();
            Case_01967();
            Case_01968();
            Case_01969();
            Case_01970();
            Case_01971();
            Case_01972();
            Case_01973();
            Case_01974();
            Case_01975();
            Case_01976();
            Case_01977();
            Case_01978();
            Case_01979();
        }

        private static void Case_01800()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1800,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-6,15,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-5,38,4,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,15,48,17,4), new GeneratedEnemyUnit(3,-1,29,15,4), new GeneratedEnemyUnit(5,-14,90,46,2), new GeneratedEnemyUnit(-14,10,35,20,4), new GeneratedEnemyUnit(-4,11,7,12,2), new GeneratedEnemyUnit(2,20,65,14,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "55ebe31e67ec10cde65acc856db7b387e7cc50e7a916f9c18c230f8734da8d23");
        }

        private static void Case_01801()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1801,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,10,53,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-20,30,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,8,6,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-15,93,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-5,80,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,8,60,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-15,19,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,-11,20,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,14,77,21,4), new GeneratedEnemyUnit(3,-20,55,32,3), new GeneratedEnemyUnit(-16,16,78,4,1), new GeneratedEnemyUnit(3,11,71,28,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "e295429c1fd28d1e5c3bf01a77440170fe06b85f388c11f8b2c88c55b4ff2411");
        }

        private static void Case_01802()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1802,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-5,94,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,6,40,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-6,40,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-7,21,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-6,64,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-18,87,3,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "7da45ce7dd7610028ba5b2adeb4dec33e62aafad814cc017e3d32dfc6266713d");
        }

        private static void Case_01803()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1803,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-9,84,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,20,10,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,12,57,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,16,77,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-17,76,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,7,68,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-16,79,5,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "86d7be53ef7935f062a58708e61aeba0766ffd3c9d98b8e09223ded020836a66");
        }

        private static void Case_01804()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1804,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-5,5,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-11,6,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,14,85,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,14,20,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-13,84,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-19,30,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,0,93,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,15,15,13,2), new GeneratedEnemyUnit(11,-4,71,10,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "99e96071a59093a3cd3628156240b42b05f80e068b9c1b05aa23c265845257fa");
        }

        private static void Case_01805()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1805,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,9,63,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,-2,53,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,0,72,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,0,31,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-19,37,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,4,46,47,2), new GeneratedEnemyUnit(-19,-1,55,42,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "ac6035497c0f50cd341c9c151d7d6aa8b845857f1b5bbac59d86ca73ad95b481");
        }

        private static void Case_01806()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1806,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-12,76,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,11,63,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,14,48,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,7,87,45,4), new GeneratedEnemyUnit(1,-15,38,40,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "2103442e6350cf078e2443102f50967a08fe1341d48981325bf62ec8bdb336e8");
        }

        private static void Case_01807()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1807,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,5,19,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-8,76,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-14,98,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,17,72,3,1), new GeneratedEnemyUnit(12,17,86,44,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "7fa58309b475ab8d2fcf5de011327257bfc60d3bf8ba9ffb445c1989a0dfdcbc");
        }

        private static void Case_01808()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1808,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,6,5,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,1,20,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,5,70,45,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "b3db691acf4047f950d0054951026f9bd809dce6ac99fc8652865e664a08914a");
        }

        private static void Case_01809()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1809,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,14,63,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-17,38,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,3,26,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-1,92,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-17,40,7,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "3c2a5d0fd57316bc29731a3ce8c52922416e4886e9b9ae712d2d96c821e42c3b");
        }

        private static void Case_01810()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1810,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-20,61,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,1,45,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,18,58,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,19,26,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,11,20,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-3,77,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-5,84,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-17,39,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-5,43,27,2), new GeneratedEnemyUnit(-6,16,98,5,2), new GeneratedEnemyUnit(1,20,5,34,2), new GeneratedEnemyUnit(-4,-4,56,44,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "a34e56718ea5a102c66fa80198ffb5a4f48d444307b49f91a28ce808f3e53272");
        }

        private static void Case_01811()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1811,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,2,99,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,0,27,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,16,9,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-5,52,30,1), new GeneratedEnemyUnit(-8,7,98,23,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "710c88f2aad2aa1eff509f0ef82c4fb367097e78e41af564b1bad56aca097d41");
        }

        private static void Case_01812()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1812,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,12,86,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,11,70,6,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "e54c90bedbc6d9b0ddfa8dcbed093a428772f403d385c02927fa326c5abf53f1");
        }

        private static void Case_01813()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1813,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,14,47,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,4,82,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,10,47,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,4,87,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,13,74,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,-13,13,2,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-20,84,7,1), new GeneratedEnemyUnit(15,6,57,34,4), new GeneratedEnemyUnit(20,-12,21,37,3), new GeneratedEnemyUnit(-15,16,75,28,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "a1000b598f7d17dacde40a9246a7163ad985cfdbaf87fe12c61d9c8fcf689685");
        }

        private static void Case_01814()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1814,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-13,44,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-14,46,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-18,86,4,2), new GeneratedEnemyUnit(-10,15,96,45,3), new GeneratedEnemyUnit(-4,-5,25,38,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "645c9b88bc7fb4e97d5d73866214f147a40550e2c1b8391a85289c07cdab3710");
        }

        private static void Case_01815()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1815,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-15,82,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-11,54,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-11,87,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-2,99,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-1,62,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-14,84,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,20,24,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-9,22,27,3), new GeneratedEnemyUnit(15,3,8,15,2), new GeneratedEnemyUnit(-20,-18,25,9,1), new GeneratedEnemyUnit(-10,13,79,44,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "b683b70fd501730a4eae6f07ecfce8961772c27d5c1eaa3500ba9e52d21ecebb");
        }

        private static void Case_01816()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1816,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,14,6,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,4,64,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,12,63,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,6,61,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-18,9,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,6,53,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,14,7,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,4,84,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-16,14,9,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "66fe25fb9114c18ec1cbdb0624adb059a1dd2d8c0b608f895f7739c315a1df47");
        }

        private static void Case_01817()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1817,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,5,5,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-9,92,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-5,69,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,17,42,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-6,37,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-13,78,30,4), new GeneratedEnemyUnit(-20,19,42,48,1), new GeneratedEnemyUnit(-11,-5,93,12,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "dcb3d80b0c94ff9ea2fd248a7ee346c214ceff28ef28c7042267cbee2fcac023");
        }

        private static void Case_01818()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1818,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,7,10,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,4,53,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,6,64,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-4,46,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-5,70,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,12,36,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-14,43,45,3), new GeneratedEnemyUnit(-14,-3,56,48,4), new GeneratedEnemyUnit(-11,-14,21,45,1), new GeneratedEnemyUnit(-13,16,64,46,2), new GeneratedEnemyUnit(20,-17,44,5,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "a19395b8cb446075fba9df8aa979c4b826f2601d7b6056ccc66add6091fd2ce8");
        }

        private static void Case_01819()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1819,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-7,54,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,19,51,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,0,11,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-2,94,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,9,77,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-11,70,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-6,20,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,11,98,41,1), new GeneratedEnemyUnit(-20,-11,6,4,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "c427a293f33f114884ac0df6ac2a9a8e92e15e76afb8335ba1480472bc3c40cf");
        }

        private static void Case_01820()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1820,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,20,67,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,18,93,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,15,11,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-11,41,33,4), new GeneratedEnemyUnit(-17,-5,17,13,4), new GeneratedEnemyUnit(-8,-12,44,18,1), new GeneratedEnemyUnit(-16,-13,58,20,4), new GeneratedEnemyUnit(-17,-3,34,40,2), new GeneratedEnemyUnit(-18,-1,6,35,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 55,
                stableHash: "841dabc57bb68611b3bc9698df1031340ebf869db1259d134f2b9ab2a1568ade");
        }

        private static void Case_01821()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1821,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,8,71,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-2,97,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,11,11,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,10,8,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-19,82,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-12,83,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,10,65,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-11,48,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,9,16,13,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 75,
                stableHash: "01abe9c803952c13592e7d9c096f206939a806f52ebd54bebe35647cae60b0de");
        }

        private static void Case_01822()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1822,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-19,77,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,8,57,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-19,92,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-10,72,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,10,90,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-11,72,2,3), new GeneratedEnemyUnit(3,18,99,1,3), new GeneratedEnemyUnit(2,5,6,44,1), new GeneratedEnemyUnit(-14,10,40,24,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "e16f7fc39a52c24dd19bbbc45b191f2ae2088394c354f0aa15e8909c1b195168");
        }

        private static void Case_01823()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1823,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,15,99,1,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "9c19bf22c5f080300f4cd2870bb842fa38e9682768fa34ee5c3e29e950287aa7");
        }

        private static void Case_01824()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1824,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,0,21,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,11,17,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-8,78,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,5,43,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,8,44,11,3), new GeneratedEnemyUnit(4,6,72,25,3), new GeneratedEnemyUnit(-11,-9,61,41,1), new GeneratedEnemyUnit(3,-17,57,6,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "7c894ff4099beaa970f5b51fe8d0ca73386f27bcb315548a9a65e50a9c6600fd");
        }

        private static void Case_01825()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1825,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,7,26,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-7,100,2,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "11bdd3194ea44a20870307e22436568b212dc6ea367adeb9b60774c65e61ea2b");
        }

        private static void Case_01826()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1826,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-10,68,3,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "c19c29a7c60ef1153c2903fcc827017b42e82df690c7e56ccf8d8321d04e7fc8");
        }

        private static void Case_01827()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1827,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,12,8,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-17,27,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-16,86,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,14,78,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-19,71,48,1), new GeneratedEnemyUnit(5,-16,90,40,2), new GeneratedEnemyUnit(-19,-19,100,48,3), new GeneratedEnemyUnit(20,11,18,28,3), new GeneratedEnemyUnit(2,18,21,27,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "223ee23c6aa37a147bc7341ee7e08dcf8711046439ebe1d1a947e3f7ddde48d3");
        }

        private static void Case_01828()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1828,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-14,96,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-20,89,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-9,93,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,4,50,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-5,38,2,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "96fe5966d8f13ec3514b0b568988507bf361b8ab075e306b51650655fe8403b7");
        }

        private static void Case_01829()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1829,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,3,66,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-11,81,9,2), new GeneratedEnemyUnit(-18,-2,41,32,3), new GeneratedEnemyUnit(-8,-2,46,39,4), new GeneratedEnemyUnit(17,19,49,5,1), new GeneratedEnemyUnit(9,18,8,46,1), new GeneratedEnemyUnit(-6,-14,95,41,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "881b42f5341f3c13b311aee0656efc5f4f5b94a9f4f81c4375905d923cab9d8c");
        }

        private static void Case_01830()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1830,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,20,88,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-13,72,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,19,66,8,2), new GeneratedEnemyUnit(-2,-4,52,25,1), new GeneratedEnemyUnit(-5,1,10,27,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "ed775f15d48f339340dfbb4b0feacb968910c937cca702e43ad7c8224eef1a29");
        }

        private static void Case_01831()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1831,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-4,88,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,9,31,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-15,55,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-9,59,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,10,27,1,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "8c51401c5e769648efaf4f8143b9ba703215097775c91be82ba6824aefd2184b");
        }

        private static void Case_01832()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1832,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,6,53,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,10,46,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,2,28,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,1,19,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-1,49,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-9,92,9,4), new GeneratedEnemyUnit(19,-4,88,30,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 25,
                stableHash: "952e6179fac07c0128fe0da8d124bc843f8e349aa134c54d5c1954ff667f8ab2");
        }

        private static void Case_01833()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1833,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-5,89,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,6,92,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,14,49,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-4,23,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,13,88,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-15,82,29,4), new GeneratedEnemyUnit(20,-16,27,8,2), new GeneratedEnemyUnit(-19,-6,78,22,2), new GeneratedEnemyUnit(18,-18,54,8,4), new GeneratedEnemyUnit(-9,-13,97,6,1), new GeneratedEnemyUnit(2,17,70,21,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "d673edfeab2542e2d269dbaebe069e664343b86eeb0cb430d8ddfe19421478ee");
        }

        private static void Case_01834()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1834,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-18,98,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-4,69,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-13,100,2,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,18,66,39,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "cbd6ef4ddfa04983033c68c82a570323794381fcfa19636d338a8d8a03530d0d");
        }

        private static void Case_01835()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1835,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-5,23,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-2,46,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-16,79,3,2), new GeneratedEnemyUnit(-7,-15,42,43,4), new GeneratedEnemyUnit(12,-4,92,33,3), new GeneratedEnemyUnit(-20,9,12,50,1), new GeneratedEnemyUnit(2,-8,68,31,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 35,
                stableHash: "868bf50d7e7a7980e58730f571f231acb8bf7effe6292a500d8b318c6f512513");
        }

        private static void Case_01836()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1836,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-8,67,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,7,22,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,15,84,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,9,18,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,2,70,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,11,85,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,5,92,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,5,48,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-17,67,39,4), new GeneratedEnemyUnit(1,-4,30,45,1), new GeneratedEnemyUnit(-19,-16,70,30,3), new GeneratedEnemyUnit(13,-10,40,27,2), new GeneratedEnemyUnit(19,-3,40,37,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "0b375050621f283eafd5746a040a606f0877e97e4c095b6e0914476febd7ec06");
        }

        private static void Case_01837()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1837,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,11,66,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,3,59,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-20,24,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,12,79,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,1,7,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,5,60,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-11,17,34,4), new GeneratedEnemyUnit(2,-9,43,14,2), new GeneratedEnemyUnit(-2,18,95,29,4), new GeneratedEnemyUnit(9,-18,37,25,3), new GeneratedEnemyUnit(10,-12,40,39,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "56d3b245a8877f0f2a01fa7ccb4adafa6f31a02cd524b92f71dfb6e2ed9d30ca");
        }

        private static void Case_01838()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1838,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,7,48,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,0,19,6,3), new GeneratedEnemyUnit(-15,16,88,27,1), new GeneratedEnemyUnit(12,4,47,50,3), new GeneratedEnemyUnit(-8,-4,8,24,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "77583731323635cc08870618cf366e39487554ae83ce47f8e67a8018b72c9aa2");
        }

        private static void Case_01839()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1839,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-3,11,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,11,34,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-6,38,22,3), new GeneratedEnemyUnit(-13,19,6,43,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "7d70193413c8bd0e9b5e8a079f023386cab04f3e525ba12fa166db285b9eb314");
        }

        private static void Case_01840()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1840,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,5,70,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-2,55,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-16,55,35,1), new GeneratedEnemyUnit(12,-15,24,8,3), new GeneratedEnemyUnit(-14,0,17,2,2), new GeneratedEnemyUnit(0,7,75,11,1), new GeneratedEnemyUnit(6,-13,54,35,4), new GeneratedEnemyUnit(9,-17,12,11,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "a3ce6d733643619c4ae4829a153d4583c2b9cc836c157396c307be51c652a862");
        }

        private static void Case_01841()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1841,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,2,39,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,0,59,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,17,81,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-7,45,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-15,27,6,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-2,77,23,1), new GeneratedEnemyUnit(-4,-1,74,9,1), new GeneratedEnemyUnit(-5,15,31,21,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "93727c3ed169bd8228ffa07f95be3fd38b7ab977c0a4f07f0688f9c76191b887");
        }

        private static void Case_01842()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1842,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,11,67,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,2,38,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,7,8,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-20,100,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-12,21,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,16,19,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,0,84,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,2,35,3,4), new GeneratedEnemyUnit(-2,7,54,46,2), new GeneratedEnemyUnit(12,-11,41,15,1), new GeneratedEnemyUnit(10,-3,22,3,1), new GeneratedEnemyUnit(-18,11,46,45,4), new GeneratedEnemyUnit(-13,-10,35,39,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "a602fe95dee42b186e0314bdd1f1926a1fdcad8b15890cd84dc94f81b9a85934");
        }

        private static void Case_01843()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1843,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-8,48,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,11,72,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-6,91,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-12,80,12,1), new GeneratedEnemyUnit(6,6,60,26,4), new GeneratedEnemyUnit(-16,-19,44,27,3), new GeneratedEnemyUnit(6,9,59,15,4), new GeneratedEnemyUnit(14,12,68,1,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "adf5736337d7c30ddfbbda6e7b1c2502265551df5a1dd91bd90277a155f0c7fb");
        }

        private static void Case_01844()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1844,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-13,47,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-9,13,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,5,36,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-15,97,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-5,54,22,1), new GeneratedEnemyUnit(18,-14,59,2,1), new GeneratedEnemyUnit(18,3,45,31,1), new GeneratedEnemyUnit(11,8,92,36,3), new GeneratedEnemyUnit(19,-1,66,17,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "9396c783fbdad39d9f962e0531764fe741b93740689cc1769b824c41d4a41842");
        }

        private static void Case_01845()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1845,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-10,12,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-16,49,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,13,67,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,7,22,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-16,74,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-13,53,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-5,91,24,1), new GeneratedEnemyUnit(0,12,40,21,2), new GeneratedEnemyUnit(-19,19,31,48,4), new GeneratedEnemyUnit(19,-13,84,40,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "d2bbac18953b5723e61c01f932be9aa73bce0c02a333d71aaa451d2d927e1a4a");
        }

        private static void Case_01846()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1846,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-6,60,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,6,67,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-1,72,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,3,70,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,18,53,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-8,39,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-5,78,18,4), new GeneratedEnemyUnit(0,16,42,6,4), new GeneratedEnemyUnit(-17,-9,96,15,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "a6c545e0ee15baca8af1069cc7f6dfb555e5cd39fc886134434ac4f61ace8cce");
        }

        private static void Case_01847()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1847,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,3,90,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-12,86,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-6,94,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,19,23,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-20,55,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-2,40,47,1), new GeneratedEnemyUnit(-4,-13,23,1,3), new GeneratedEnemyUnit(-11,-18,74,46,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "76c278322aca9354b6cb796c52bad8c35a3f1ce3db1c32f5d6f86954f8c3a217");
        }

        private static void Case_01848()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1848,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-8,39,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,0,17,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-13,35,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-14,78,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-2,93,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,3,91,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,2,93,2,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,14,79,41,1), new GeneratedEnemyUnit(10,11,93,2,3), new GeneratedEnemyUnit(-5,16,48,32,2), new GeneratedEnemyUnit(8,-3,77,4,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "886285ba1b5930e596935d1023237811bd89c5ef742dddc61ff27b20dd8a6920");
        }

        private static void Case_01849()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1849,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-15,95,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-6,49,3,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "92b1a4dcb142119d58d0b6aaac7b3a1820c7e77f7557cadf6c1bb2e5b593dc71");
        }

        private static void Case_01850()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1850,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,6,29,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-13,82,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-15,100,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,5,21,43,2), new GeneratedEnemyUnit(-12,-17,22,8,3), new GeneratedEnemyUnit(-3,3,100,44,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "06c6123a939feb811af6a51c0175f38bf51eefad2a791a810f53aadf8c4e37c5");
        }

        private static void Case_01851()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1851,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,4,26,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,13,78,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,15,32,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,4,44,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,11,63,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,7,53,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,8,51,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,5,54,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,14,27,43,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "7c1b7acabb797f3665c81be693974e75cf4cf77cd2d1c17684044e8c3a14fab5");
        }

        private static void Case_01852()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1852,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-4,96,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,10,78,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-18,91,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-16,7,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,15,73,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-9,39,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,2,45,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,7,54,42,1), new GeneratedEnemyUnit(-19,3,56,50,2), new GeneratedEnemyUnit(1,2,20,1,1), new GeneratedEnemyUnit(-8,-8,5,10,3), new GeneratedEnemyUnit(19,3,42,20,4), new GeneratedEnemyUnit(9,17,25,37,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "7f60ed2249161a0971298b09e04e46b675b185bd48b6be869c5a1f50d20f8a52");
        }

        private static void Case_01853()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1853,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-9,39,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,18,39,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-5,18,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-8,89,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-12,70,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-11,5,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-16,80,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,0,37,20,1), new GeneratedEnemyUnit(-2,2,13,34,1), new GeneratedEnemyUnit(-9,0,68,33,1), new GeneratedEnemyUnit(12,13,35,4,2), new GeneratedEnemyUnit(0,3,35,16,1), new GeneratedEnemyUnit(2,-5,89,42,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "c87f0824dff1df6a3137841181f9837d8a13edaa70a89aa93614f9978740037f");
        }

        private static void Case_01854()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1854,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-14,42,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,5,6,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,6,24,45,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "03d73a851a8b5b1bb00ef6a43337ee399fac3eee7693e9816f368321fb18d6b7");
        }

        private static void Case_01855()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1855,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-15,12,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-5,11,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,19,24,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,10,93,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,16,13,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-12,31,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,10,81,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-1,59,30,3), new GeneratedEnemyUnit(1,20,22,49,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "a829fff87593f07405b8efe21a3fcafce3832b572ad4a0326330abb20d064236");
        }

        private static void Case_01856()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1856,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,12,6,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-14,45,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,4,44,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,19,5,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-14,23,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,5,18,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,3,19,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-13,8,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-9,20,42,3), new GeneratedEnemyUnit(-20,-11,54,19,3), new GeneratedEnemyUnit(-12,-5,24,11,4), new GeneratedEnemyUnit(5,-12,60,6,4), new GeneratedEnemyUnit(-2,-8,12,27,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "c386b06893838fa309e0f2a05e3aea0ca746a4ebc58b5b0724d691495417f434");
        }

        private static void Case_01857()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1857,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-8,7,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-4,43,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,8,38,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,-20,42,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-17,82,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-14,77,24,4), new GeneratedEnemyUnit(11,-1,77,36,4), new GeneratedEnemyUnit(-5,-16,70,11,2), new GeneratedEnemyUnit(9,-16,51,25,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "06b379ee5d17429110b036dbc5a239afedd9d53b230b68681e7cc7ddfa96d8c2");
        }

        private static void Case_01858()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1858,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-10,15,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-19,22,37,3), new GeneratedEnemyUnit(6,20,10,29,3), new GeneratedEnemyUnit(-3,-9,51,45,3), new GeneratedEnemyUnit(-8,-11,59,6,4), new GeneratedEnemyUnit(-1,-16,43,18,2), new GeneratedEnemyUnit(10,3,63,6,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "b4011b856ea3a953d8e43aa913f2357817574924fd9aa44f50e6599a31133690");
        }

        private static void Case_01859()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1859,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,11,84,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,7,79,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-18,80,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-13,72,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,19,37,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,16,77,18,2), new GeneratedEnemyUnit(-5,18,69,23,1), new GeneratedEnemyUnit(1,5,13,42,4), new GeneratedEnemyUnit(7,-17,19,24,3), new GeneratedEnemyUnit(16,16,31,40,1), new GeneratedEnemyUnit(8,15,81,22,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "be487f7250bf6ecbd6abcb265bb1f2e3ca4dda34a780dc76729f4de824373ff3");
        }

        private static void Case_01860()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1860,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-8,43,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-4,27,30,1), new GeneratedEnemyUnit(20,19,5,6,3), new GeneratedEnemyUnit(20,15,56,16,1), new GeneratedEnemyUnit(20,-16,9,39,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "b28f7fdc8ad4b118725b0f8445f0f810df7084580a2ca5315c1063bb71595c6d");
        }

        private static void Case_01861()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1861,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-11,19,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-8,75,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,10,66,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-19,76,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,17,44,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-11,7,46,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "d8346d6eff845c0a8de24f942806abed932de277fce4e90ce3aed0e7ac0e68e2");
        }

        private static void Case_01862()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1862,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-14,73,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,16,75,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,8,72,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-5,94,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,3,87,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-17,68,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-20,84,1,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "b41c1389824639686f5c9a8f4306ca253fdf2542c0aadf4a53ca26a2c494294f");
        }

        private static void Case_01863()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1863,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,7,36,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,8,26,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-9,99,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,2,25,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,3,50,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-5,55,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-19,32,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,18,52,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-14,12,2,4), new GeneratedEnemyUnit(-19,18,45,49,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "9a9af727e4a0094a267c1c04d3539d09abd4f99658b704cd0a53a258aee508b3");
        }

        private static void Case_01864()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1864,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-1,31,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-13,14,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-16,47,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-7,21,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,5,47,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-7,15,3,4), new GeneratedEnemyUnit(-1,6,58,47,1), new GeneratedEnemyUnit(13,-5,42,13,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "8211685f6862905835e2d83d33e8df1f3679c17c5d226d51db891f6743b91360");
        }

        private static void Case_01865()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1865,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-6,37,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,12,79,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,10,48,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,7,39,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-1,51,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,2,28,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,7,31,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,16,20,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,16,50,43,4), new GeneratedEnemyUnit(-1,-2,29,30,2), new GeneratedEnemyUnit(11,-9,77,50,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "df4cc68c2cb7e487ca58d7af7444c8ca8bd9e2396823df2e287e2e85d748757e");
        }

        private static void Case_01866()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1866,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,17,40,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,6,54,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-5,8,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,15,13,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,7,31,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-9,29,7,2), new GeneratedEnemyUnit(4,-8,66,32,1), new GeneratedEnemyUnit(18,9,64,5,4), new GeneratedEnemyUnit(-11,0,17,17,1), new GeneratedEnemyUnit(-16,20,42,20,1), new GeneratedEnemyUnit(-8,-3,24,38,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "343092872997225fc3187bad1654d85bbe94c77a904d5b5f216526a9ae0ba52a");
        }

        private static void Case_01867()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1867,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-11,15,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-19,76,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,10,15,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,16,69,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,10,59,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-12,66,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,3,26,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-16,39,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-2,80,21,3), new GeneratedEnemyUnit(-14,6,70,12,4), new GeneratedEnemyUnit(-14,19,25,34,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "3c320bb60833282f6bc661feec45a3a80f1e42f7466d7ee852183036474c75df");
        }

        private static void Case_01868()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1868,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-16,72,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,7,53,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,3,34,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-17,71,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,6,87,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,0,98,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,14,72,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,19,26,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,5,40,49,3), new GeneratedEnemyUnit(4,20,36,23,4), new GeneratedEnemyUnit(-12,-17,17,44,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "58338a99823fd0a37020828cbf834feae64a96500daa828c13c799421f810c26");
        }

        private static void Case_01869()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1869,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-11,87,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-15,6,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-7,86,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,18,65,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-10,67,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-20,61,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,6,43,14,2), new GeneratedEnemyUnit(-4,-11,36,16,3), new GeneratedEnemyUnit(-2,18,67,40,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "3e472c70c6fac25ec23a28c1f81e46970504627b9cbd5e329d484d13af8b485c");
        }

        private static void Case_01870()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1870,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-2,63,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,6,41,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-7,14,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,18,35,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,0,58,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,9,85,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,7,93,20,4), new GeneratedEnemyUnit(-19,-1,79,16,3), new GeneratedEnemyUnit(-9,-14,93,45,2), new GeneratedEnemyUnit(-20,-12,84,37,2), new GeneratedEnemyUnit(1,11,18,28,4), new GeneratedEnemyUnit(15,15,48,26,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "7b68629412c8c849613207c4f0cee8de69bcd8490441bb1dfa8d424c09ecf745");
        }

        private static void Case_01871()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1871,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-1,5,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-8,39,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-18,58,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-7,72,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-17,59,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,19,11,24,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "1e4ce1bea69889857999e6d4fc12df1ff834b7d58121099789aec5bba81178e8");
        }

        private static void Case_01872()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1872,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,18,33,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-10,97,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,17,98,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-2,35,4,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-8,54,30,3), new GeneratedEnemyUnit(7,11,55,50,4), new GeneratedEnemyUnit(9,12,56,24,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "dcd33c3d30418189c02c4e7b3a5fb5de01ef5437ac481e295acd8d0c397f9cf7");
        }

        private static void Case_01873()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1873,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-11,26,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,0,31,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-18,66,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-8,77,7,1), new GeneratedEnemyUnit(18,-15,53,4,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "6938ff3d53e54a4e81de6ea16a5eeb4702750ad8676567cd89182721830c613c");
        }

        private static void Case_01874()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1874,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-6,58,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,11,32,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-6,36,4,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "f727fcdb3976e239c8ea86d868e2786693353a691e1fe7b3dac2e31667d27a0a");
        }

        private static void Case_01875()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1875,
                posture: BotStrategicPosture.Search,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,15,22,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,6,20,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-15,47,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,6,75,6,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "3a5fff40eed681dd7dd763bc8dc1c2a73a83aa5196172f5f4602e959f23b8c8d");
        }

        private static void Case_01876()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1876,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-8,76,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,12,61,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-20,60,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,3,78,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,18,42,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,15,100,50,1), new GeneratedEnemyUnit(-11,9,53,3,1), new GeneratedEnemyUnit(-16,-16,17,34,2), new GeneratedEnemyUnit(13,-7,68,4,4), new GeneratedEnemyUnit(-1,1,70,7,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "cec9bcb6c01c861a2a47d238a700970b62c11f9549ff04ccd1c490ba6a8189af");
        }

        private static void Case_01877()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1877,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,17,63,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-19,7,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-14,91,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-19,56,6,4), new GeneratedEnemyUnit(18,-12,82,45,1), new GeneratedEnemyUnit(7,6,88,34,4), new GeneratedEnemyUnit(-19,13,47,3,4), new GeneratedEnemyUnit(-18,-8,84,32,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "8c0f633ed17797fc7c27a7adefd0f130d5871b9ff1ffe00126d9aad7a9670dbd");
        }

        private static void Case_01878()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1878,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,6,28,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,16,92,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-20,74,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-1,20,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-13,72,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-15,93,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,4,6,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,-2,47,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,17,54,28,2), new GeneratedEnemyUnit(3,-3,75,22,2), new GeneratedEnemyUnit(9,14,32,37,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "db7aea3045f35a440afec028e952c527888a3594569a29f2e755ff1c49721184");
        }

        private static void Case_01879()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1879,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-10,42,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-4,11,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,3,86,26,4), new GeneratedEnemyUnit(-9,-14,21,31,4), new GeneratedEnemyUnit(5,-10,71,10,1), new GeneratedEnemyUnit(11,-19,82,41,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "274489afd91216154053de247afe1649b5370ca8547826f6e54a95fb9f048e8b");
        }

        private static void Case_01880()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1880,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-12,65,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-6,33,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,18,59,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,8,65,24,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 75,
                stableHash: "d2b3c13384f90ab12cf18b7e54c6225fe108ce8b50324c65ce56da18730eb09e");
        }

        private static void Case_01881()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1881,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,15,15,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-13,75,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,13,66,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-1,26,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,1,51,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-10,62,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-3,68,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,20,61,3,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "8f1cf3a728547020e7dec7b4a755f055b8dd92ded4088df8e64c14a2eb4b92c6");
        }

        private static void Case_01882()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1882,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-14,57,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,0,7,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-19,32,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-2,85,49,4), new GeneratedEnemyUnit(-2,2,94,20,3), new GeneratedEnemyUnit(12,5,70,37,4), new GeneratedEnemyUnit(10,-14,43,49,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "fd759ed1c48c31b0f2794c37f3597b76671a70702e42196dfa31675dc831f5bd");
        }

        private static void Case_01883()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1883,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,8,76,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,17,27,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-4,81,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,4,10,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,11,93,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,14,69,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,13,11,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,10,88,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,15,55,10,3), new GeneratedEnemyUnit(-7,-7,12,44,1), new GeneratedEnemyUnit(-19,5,26,3,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "4e744043da831f8b744a586288045606076fec5c7abfbe404086ab1d1f3985fa");
        }

        private static void Case_01884()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1884,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,0,82,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-18,5,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,4,90,5,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "8f8ba82a2b2f6d3873ffeeca32efd5807c4475dbe9bc10e4fa96ff45a4599ade");
        }

        private static void Case_01885()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1885,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-3,11,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-16,39,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-6,84,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-1,76,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-1,84,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-1,73,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,2,60,35,2), new GeneratedEnemyUnit(12,-20,100,1,1), new GeneratedEnemyUnit(-10,-17,25,3,2), new GeneratedEnemyUnit(19,-11,63,17,3), new GeneratedEnemyUnit(11,17,47,41,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "700f7d3bb1cc209c9e28f8c5397cfdd70f03a4bc4cd694d08bc090b3bcec7631");
        }

        private static void Case_01886()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1886,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-6,10,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,16,83,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,7,28,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-14,82,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,6,20,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,0,37,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-10,55,17,3), new GeneratedEnemyUnit(-18,-20,23,47,1), new GeneratedEnemyUnit(3,0,34,35,3), new GeneratedEnemyUnit(12,9,93,27,1), new GeneratedEnemyUnit(-18,17,88,3,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "61695288473d53804486ef1ae2047e2aa2c7c896a25d7cb32c175fb4ebec5fd9");
        }

        private static void Case_01887()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1887,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-16,29,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-11,75,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-11,68,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,6,55,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,2,28,3,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,10,74,25,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "d9f19e77e4ca49dcbe30e9d0c68362c6ac2fbf06b51bae289003ed7468242b46");
        }

        private static void Case_01888()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1888,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-12,92,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-17,62,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-20,90,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-11,98,33,2), new GeneratedEnemyUnit(9,17,60,42,2), new GeneratedEnemyUnit(11,19,68,8,1), new GeneratedEnemyUnit(3,17,57,32,2), new GeneratedEnemyUnit(3,-10,8,29,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "12b77e5d2327dbc70963e5fef29e6887df7c501ef528cdf6a7d127fa5b86fd94");
        }

        private static void Case_01889()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1889,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,8,8,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-12,7,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,1,84,24,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "d865db5b08b6c0cfe05895c4e73e2ddf1d709987910ae051abb853bff5ea74da");
        }

        private static void Case_01890()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1890,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,15,30,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-7,96,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-10,96,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-19,35,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,20,96,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,5,7,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-12,30,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-4,34,1,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "0f542f690e9c8e5ace7a955ff19f7f6f8ee2280b881976effb4aa666da55fde0");
        }

        private static void Case_01891()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1891,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-11,64,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,14,77,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,4,24,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-17,16,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-4,85,43,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "0594566318e1e817262be4892fcc81151ed1fb1816e031a45e643570d3d104e8");
        }

        private static void Case_01892()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1892,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-4,42,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,10,98,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,20,97,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,10,79,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-10,36,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-5,67,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,10,89,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-7,59,35,3), new GeneratedEnemyUnit(6,-19,81,49,4), new GeneratedEnemyUnit(-17,-5,59,23,3), new GeneratedEnemyUnit(-17,-2,96,38,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "11459767ffe6a4aeb4ec3e48d48c67a3ca5b7743aeb33f90deba18bb32d0f89b");
        }

        private static void Case_01893()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1893,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-20,31,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,14,85,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-15,46,6,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-4,6,44,3), new GeneratedEnemyUnit(19,0,8,10,4), new GeneratedEnemyUnit(-11,-15,96,47,3), new GeneratedEnemyUnit(5,-11,48,13,4), new GeneratedEnemyUnit(17,3,19,50,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "32c1ad17280e5d550a4b59b1e3ef9e0b5d936e5f138ed2db02e25e024b8c06d7");
        }

        private static void Case_01894()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1894,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-7,57,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,14,27,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,3,79,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,12,89,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,13,86,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-17,30,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,8,68,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-11,85,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,15,69,34,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "b8a3c880333de35ab3c364c93e43f243786dcffdcbf0be9d995d596808609a35");
        }

        private static void Case_01895()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1895,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-10,98,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-7,30,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,2,18,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,3,99,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,16,78,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,16,27,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,20,31,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,2,39,2,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,1,46,43,2), new GeneratedEnemyUnit(9,-7,91,38,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "9528237501f6cb4cdee63f53da26412777ed4a183eea05853e901c6bffd7b3fc");
        }

        private static void Case_01896()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1896,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,18,83,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,18,67,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-10,77,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-11,83,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-5,59,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-7,21,7,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,11,43,1,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "263ba312adf1961a12bb9cadfaf71d561bee97fb4645ba64e741c4b957f3138c");
        }

        private static void Case_01897()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1897,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-14,44,5,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "55592a390ec07c6018f58728844b9df009b1985cd5bf550cbc51996453eb372c");
        }

        private static void Case_01898()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1898,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,11,29,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,2,6,2,1), new GeneratedEnemyUnit(-6,5,28,26,1), new GeneratedEnemyUnit(-18,0,46,28,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "a263472c0d39fd4b195afdd9901938b9dfe846191a8bcb3f4416f340cebbb12f");
        }

        private static void Case_01899()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1899,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-19,57,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,4,44,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,11,28,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-16,58,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,12,82,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-16,26,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,18,86,48,2), new GeneratedEnemyUnit(-14,7,30,24,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "447109481f718d8ec3711a64384b5e05fe2417b39380b051f53aca747cc53414");
        }

        private static void Case_01900()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1900,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,18,97,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-17,92,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,8,78,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,10,83,41,3), new GeneratedEnemyUnit(-4,-18,69,10,2), new GeneratedEnemyUnit(-4,13,91,32,2), new GeneratedEnemyUnit(-8,6,40,31,4), new GeneratedEnemyUnit(17,-3,55,25,1), new GeneratedEnemyUnit(19,-8,45,43,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 90,
                stableHash: "2236bdfa05218f779d01382c52459dfdcf70f1204a5e25b16e1609bb98504288");
        }

        private static void Case_01901()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1901,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,11,8,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,9,17,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,17,48,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,11,68,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,3,75,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-13,100,14,2), new GeneratedEnemyUnit(15,17,99,11,4), new GeneratedEnemyUnit(15,16,59,4,2), new GeneratedEnemyUnit(-18,18,56,29,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "daaf1d3d9ea00bd6a32501888a1aa0c00377928b35bb6cb485c5212d88fa4329");
        }

        private static void Case_01902()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1902,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,16,42,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,17,89,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-6,15,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,14,48,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-20,25,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-10,91,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,1,84,27,3), new GeneratedEnemyUnit(14,-8,63,26,1), new GeneratedEnemyUnit(-17,-19,72,38,1), new GeneratedEnemyUnit(-4,-20,23,15,2), new GeneratedEnemyUnit(-4,2,43,31,3), new GeneratedEnemyUnit(-6,3,36,35,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "1f4dfdecc566fc01d2ac43f3896150be09564d38535067e1e0e159568561f5bb");
        }

        private static void Case_01903()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1903,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-12,87,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,3,40,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-2,62,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-2,44,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,1,40,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-20,6,45,3), new GeneratedEnemyUnit(19,-4,8,9,2), new GeneratedEnemyUnit(0,0,87,46,1), new GeneratedEnemyUnit(-11,-7,45,12,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "cc930a84dae4accbfcd86163bc73eba2c64b40bec15049a00fc1f80d6a1c65b8");
        }

        private static void Case_01904()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1904,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-19,97,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-20,91,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,14,83,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-9,61,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,2,47,8,4), new GeneratedEnemyUnit(15,-1,10,8,2), new GeneratedEnemyUnit(18,-13,91,6,2), new GeneratedEnemyUnit(6,-16,14,32,3), new GeneratedEnemyUnit(0,-1,73,35,4), new GeneratedEnemyUnit(7,-9,21,25,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "9f6568cfa3a6b34e10600de31d16fb3e4e2cd3eca4927f7cdc3a03020952824e");
        }

        private static void Case_01905()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1905,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-20,17,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-14,52,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,10,7,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,16,28,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-18,36,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,12,54,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-20,99,14,1), new GeneratedEnemyUnit(17,9,64,19,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "c61bcc7d25a6a25b6be94ab4f8aac309b90acd62f39df37ef1ab3bd2afcc355b");
        }

        private static void Case_01906()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1906,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-14,86,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,5,76,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,2,60,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-13,10,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,15,42,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-1,62,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-17,52,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,18,78,1,3), new GeneratedEnemyUnit(-3,19,77,16,2), new GeneratedEnemyUnit(-20,-5,73,22,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "44845a06d80d81a1894976c6c81792cc7657f777c3cfc96dbd8e6470adab7aed");
        }

        private static void Case_01907()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1907,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,3,86,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,4,89,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-11,61,34,1), new GeneratedEnemyUnit(-10,-13,51,14,2), new GeneratedEnemyUnit(1,-16,44,36,2), new GeneratedEnemyUnit(-17,6,86,41,2), new GeneratedEnemyUnit(20,3,62,37,2), new GeneratedEnemyUnit(-9,8,27,5,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "5969e330f0ff6bfb1541ad71c99ee75abbd1fd0798a6db332395d9688af2df06");
        }

        private static void Case_01908()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1908,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,1,63,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,13,72,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,14,61,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-3,70,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-5,93,49,2), new GeneratedEnemyUnit(-13,-18,35,22,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "0adc542a4d3d93863f747fe25e78ba8f4544d15107f693a42817efceb4cf9af4");
        }

        private static void Case_01909()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1909,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-2,6,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,19,32,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,8,6,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,9,32,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,19,88,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,12,24,30,1), new GeneratedEnemyUnit(-11,18,60,46,2), new GeneratedEnemyUnit(-7,17,79,37,4), new GeneratedEnemyUnit(-20,-1,79,41,1), new GeneratedEnemyUnit(-5,-5,88,39,2), new GeneratedEnemyUnit(-19,-13,75,26,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "dab88875180efdca3f8725a9851ee65f7a626435a5a7abb2a9037a04ee504683");
        }

        private static void Case_01910()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1910,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,11,81,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-10,37,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-9,22,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-12,83,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,11,53,3,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "a05dbb2bc6f60dd33dec3a95618dbaa1c473825e83758b8386da0fbf78315ddc");
        }

        private static void Case_01911()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1911,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-16,77,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-17,58,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-12,47,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-4,94,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-6,88,11,4), new GeneratedEnemyUnit(12,19,15,19,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "3900bcdbb31b9303ac3af142fc2ec418800dabbd645174e09f209fe37b31698a");
        }

        private static void Case_01912()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1912,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,15,98,7,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,20,90,1,3), new GeneratedEnemyUnit(-16,6,32,47,4), new GeneratedEnemyUnit(-4,-4,30,49,3), new GeneratedEnemyUnit(20,0,54,34,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "6d6f0361ed6c3e37f010e3e7cd25d6062b0d5a89c72182dd316b9a6805cf54ff");
        }

        private static void Case_01913()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1913,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,14,44,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,2,7,6,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "7c9a181b6d8e9bb020dce6085ef2d92ce9333e46029d7c56881564676e48fefe");
        }

        private static void Case_01914()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1914,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-20,31,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-2,6,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-4,78,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-5,38,10,2), new GeneratedEnemyUnit(-20,18,96,7,3), new GeneratedEnemyUnit(-13,-20,84,34,4), new GeneratedEnemyUnit(-20,-11,21,41,4), new GeneratedEnemyUnit(-18,11,35,50,1), new GeneratedEnemyUnit(10,-12,77,41,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "072a7f235dbc064b8eac494de9861509faec88f1861411690f7ef6922eaab852");
        }

        private static void Case_01915()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1915,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-3,42,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,4,12,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-8,82,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-20,55,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,1,90,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,17,53,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,1,98,1,4), new GeneratedEnemyUnit(-4,8,32,11,1), new GeneratedEnemyUnit(16,13,31,43,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "6de85ab560542b43bf12068b0b0db620502d21b1cfaf92df0a3a3c269e64c98d");
        }

        private static void Case_01916()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1916,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-12,11,7,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "0728068db117c65676430d41d2b8dcffdb6819761104e328064ff601f6161597");
        }

        private static void Case_01917()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1917,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-12,56,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,15,5,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,0,57,43,4), new GeneratedEnemyUnit(18,14,40,26,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "3c3cf2b68589c2c1f13a50970a77ee543b73bef670d9ff88123c7f69b2e295b4");
        }

        private static void Case_01918()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1918,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,7,42,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,3,64,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-9,78,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,5,67,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-3,43,4,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-12,62,50,3), new GeneratedEnemyUnit(-16,-4,82,4,2), new GeneratedEnemyUnit(-18,-4,17,48,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 85,
                stableHash: "5675aea7915a91f96f84b658af8fca60b451e5f187f2cbc70f098fef40094dd9");
        }

        private static void Case_01919()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1919,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-5,73,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,8,95,2,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "a6d1a18e5186b9d8d4ab74126d0912b32f4d8111c6a232a2afac4f491a0fb470");
        }

        private static void Case_01920()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1920,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,18,75,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-7,73,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-8,92,6,4), new GeneratedEnemyUnit(-12,-5,69,4,2), new GeneratedEnemyUnit(-8,-3,54,15,1), new GeneratedEnemyUnit(6,-10,70,39,4), new GeneratedEnemyUnit(-2,14,37,44,2), new GeneratedEnemyUnit(16,-16,76,28,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "8d57bbb31ae830e11bed5650c4dd3a8e0a14b44013ee79e68e2d0b344fb96ab9");
        }

        private static void Case_01921()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1921,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-12,27,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,19,78,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-16,44,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,9,27,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,8,53,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,4,80,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-7,32,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-14,25,26,2), new GeneratedEnemyUnit(-8,-18,100,17,4), new GeneratedEnemyUnit(-3,6,20,18,4), new GeneratedEnemyUnit(17,-4,62,7,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "d1d7736d000ada9efae22e964b6d8e52bc794e275bb942a93846d481105586b0");
        }

        private static void Case_01922()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1922,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-12,57,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-13,56,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,7,23,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-15,54,7,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,19,36,45,4), new GeneratedEnemyUnit(8,-16,64,21,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "c0ecd405a74913a975bb81529ef2bc80e911805decc2d0f62d41a5c7d49b06f4");
        }

        private static void Case_01923()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1923,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,16,51,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-13,28,33,1), new GeneratedEnemyUnit(-2,-18,99,40,1), new GeneratedEnemyUnit(0,10,100,5,2), new GeneratedEnemyUnit(-13,-15,12,20,1), new GeneratedEnemyUnit(2,5,21,31,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "5135857965bf586fb019daf61cb51b35f269bc8f10fe9b9ac05db2a432860c6e");
        }

        private static void Case_01924()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1924,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,5,55,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,17,73,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-4,40,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-14,95,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,7,51,11,2), new GeneratedEnemyUnit(0,-15,75,27,2), new GeneratedEnemyUnit(-20,-1,74,22,1), new GeneratedEnemyUnit(19,12,31,50,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "227505e61bce8e1b26bb1d390336113f60bbe5e226161c94289e93aad1e08af6");
        }

        private static void Case_01925()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1925,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-13,99,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-14,46,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-16,13,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,13,78,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,14,46,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,9,15,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-3,46,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-16,6,7,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "c6a3924af044a07a50e11f5b1b87b04ad494d31cd4da98298ecfe29f897c2293");
        }

        private static void Case_01926()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1926,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,14,31,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-6,48,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-17,34,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-18,11,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-2,56,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-6,86,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,8,47,41,2), new GeneratedEnemyUnit(-19,-5,13,34,2), new GeneratedEnemyUnit(-3,-9,10,27,4), new GeneratedEnemyUnit(3,-19,13,41,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "67e2486adc502b931e3d7acbafc60626b285dbdcd99212f023bc5f384e9b41d4");
        }

        private static void Case_01927()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1927,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-1,52,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-13,40,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-11,57,50,2), new GeneratedEnemyUnit(-1,12,26,4,2), new GeneratedEnemyUnit(5,-14,46,23,4), new GeneratedEnemyUnit(-18,-8,90,15,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "86c905e44d93ad27faecdeb8f78e51f4b9749f47e72fdefd85aa71723eb5213f");
        }

        private static void Case_01928()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1928,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-2,43,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-6,51,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,18,89,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,9,71,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,5,36,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,8,65,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,10,69,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-5,92,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,1,68,15,2), new GeneratedEnemyUnit(18,10,77,44,2), new GeneratedEnemyUnit(6,-5,98,47,2), new GeneratedEnemyUnit(-18,1,49,15,2), new GeneratedEnemyUnit(-4,10,97,42,4), new GeneratedEnemyUnit(17,6,96,49,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "3ff68279b669545c773c9f6ab85ef16b2408b238240ad3c18813312167cd8e71");
        }

        private static void Case_01929()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1929,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,12,26,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-19,93,21,2), new GeneratedEnemyUnit(-9,-6,20,44,3), new GeneratedEnemyUnit(-8,19,42,12,4), new GeneratedEnemyUnit(0,-20,91,15,4), new GeneratedEnemyUnit(-20,13,53,44,2), new GeneratedEnemyUnit(-10,2,6,39,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "7fa8844ae84de5700eacda406206b6357c02c7becde101f25dd8dbafbc00e450");
        }

        private static void Case_01930()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1930,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,8,52,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,16,52,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-11,68,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,19,23,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,2,50,48,4), new GeneratedEnemyUnit(-1,9,96,1,3), new GeneratedEnemyUnit(-18,-13,5,40,3), new GeneratedEnemyUnit(-1,-8,19,12,3), new GeneratedEnemyUnit(7,-20,33,41,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "8e8dba01e15b5ad5dcebfd38142d545f59a00389eebc0ed09c7ea5917f146b75");
        }

        private static void Case_01931()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1931,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,7,17,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,11,22,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,16,83,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-19,85,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,-15,12,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-5,14,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-10,66,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,11,32,46,4), new GeneratedEnemyUnit(13,7,32,22,3), new GeneratedEnemyUnit(-7,14,21,29,1), new GeneratedEnemyUnit(15,-20,22,41,3), new GeneratedEnemyUnit(4,20,88,8,3), new GeneratedEnemyUnit(18,16,92,48,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "d122e6ad11bc5ed3e987d1c5dc2b721bc74e6e8fb5b5c090aff6cfd780497d72");
        }

        private static void Case_01932()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1932,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-10,41,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,3,28,14,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "0f95b873f43caabe2e88861bd053465075ab3c7c3e760cc0edd1e457edd30c45");
        }

        private static void Case_01933()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1933,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,15,6,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-15,8,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,11,21,3,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-18,64,32,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "f8f28e3df3f3947c8f392da0bf34aa5e8f0f0f761374e4bef1c3f477290646c5");
        }

        private static void Case_01934()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1934,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-5,41,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-18,33,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,6,47,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-16,45,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-7,84,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-1,47,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-2,8,3,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "29f970d6bcc6d0664bce1fcad4c84c55254938c114c9d48229aba83f6bc5e195");
        }

        private static void Case_01935()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1935,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,0,67,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,11,85,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-16,76,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,4,70,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,0,39,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-6,34,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,7,72,47,4), new GeneratedEnemyUnit(-15,-6,44,50,1), new GeneratedEnemyUnit(14,-12,37,40,3), new GeneratedEnemyUnit(-10,0,9,16,1), new GeneratedEnemyUnit(-7,9,45,2,4), new GeneratedEnemyUnit(-5,-14,61,50,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "ad9e506240a8fd3c38fc59be258b9f5f27b0c6f0550c84783b6c546a9f0bbdcc");
        }

        private static void Case_01936()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1936,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,-18,92,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-3,15,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,11,86,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-5,57,44,3), new GeneratedEnemyUnit(-2,-19,34,12,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "aa2993eb9df0d56f105db0e45a974bcdef8c5c7c3adc05acd065257c9a12e47b");
        }

        private static void Case_01937()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1937,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-5,100,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-6,49,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-17,88,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-18,19,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-9,59,4,4), new GeneratedEnemyUnit(-16,11,89,42,1), new GeneratedEnemyUnit(-10,-2,88,1,4), new GeneratedEnemyUnit(-9,-7,20,50,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "4edeae671783053989ec3d25cb179c96db3aa09c0d57ed8c8b096b9207c82235");
        }

        private static void Case_01938()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1938,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,15,96,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-13,17,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,14,13,32,4), new GeneratedEnemyUnit(14,9,81,46,4), new GeneratedEnemyUnit(16,6,18,10,2), new GeneratedEnemyUnit(19,-18,91,41,4), new GeneratedEnemyUnit(3,-6,67,34,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "49dc2904c7837de2630c3f6195efa0cfeca6d871727e64135f2ee375c573a829");
        }

        private static void Case_01939()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1939,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,11,19,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-17,34,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,19,39,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-14,59,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-5,48,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-13,55,46,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "856e362b604a21be4c49380cff06eef54d0fe6396f80f49bc0e1d75b665e7bf7");
        }

        private static void Case_01940()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1940,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,10,25,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,18,19,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,2,59,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-12,94,31,2), new GeneratedEnemyUnit(12,3,35,13,4), new GeneratedEnemyUnit(5,-8,36,26,3), new GeneratedEnemyUnit(-12,9,66,45,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 35,
                stableHash: "5564cf5941b7569195af15c607f27994c028f94c5344ea3f31fef0780672bba1");
        }

        private static void Case_01941()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1941,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-5,83,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,4,44,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-3,39,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,9,61,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,10,88,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-17,98,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,11,13,1,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,11,52,26,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "3a6b36c44257d6c570539549f596051fa0196160d14f478b715736f3c20d97bc");
        }

        private static void Case_01942()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1942,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-6,70,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-12,38,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-10,30,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,13,68,11,3), new GeneratedEnemyUnit(10,12,26,5,3), new GeneratedEnemyUnit(17,12,28,47,4), new GeneratedEnemyUnit(5,11,12,44,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "fd67a35397bc2402c633b326f6b95b2ef0beb873b741db1eef6be8f3a83ec074");
        }

        private static void Case_01943()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1943,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,13,61,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-1,40,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,1,33,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,8,90,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-6,34,36,1), new GeneratedEnemyUnit(15,-4,23,1,4), new GeneratedEnemyUnit(-19,12,62,27,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "0c416f52cf87061b4ef41e85f7d74b21c9ba08338bb611313dee79d5f41538cb");
        }

        private static void Case_01944()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1944,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,4,39,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,7,53,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-6,9,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,9,13,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,11,78,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,0,86,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,12,21,21,1), new GeneratedEnemyUnit(20,16,39,25,3), new GeneratedEnemyUnit(-2,17,85,26,4), new GeneratedEnemyUnit(-13,-15,38,18,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "15e8b781e61c162ef575721c9edc2a8be852db298bcbb6a7002f8604decb3f2f");
        }

        private static void Case_01945()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1945,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-18,62,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,8,7,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,-3,37,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-11,92,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,7,41,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,5,57,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-13,37,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,16,29,49,3), new GeneratedEnemyUnit(-15,-7,70,11,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "e5e0d4622a95d148ddb3172fd46dec67338caa29dd65c87dd2cee18ea2317a5b");
        }

        private static void Case_01946()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1946,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,8,18,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-17,90,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-17,51,19,2), new GeneratedEnemyUnit(-6,11,56,43,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 25,
                stableHash: "96628b695d4348ece9dfd3050ae7ada06fe9fb5a8e4e0ba39a081183536fb37d");
        }

        private static void Case_01947()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1947,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-9,70,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,1,48,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,12,99,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,0,27,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-11,7,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-20,37,7,3), new GeneratedEnemyUnit(-12,17,82,40,3), new GeneratedEnemyUnit(-9,-6,93,2,1), new GeneratedEnemyUnit(-8,-8,10,38,4), new GeneratedEnemyUnit(-7,6,21,18,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 35,
                stableHash: "a95c063176e81c1b432399fa256ab294d0cc25c55e1ccb3a9f9173e3fb274a53");
        }

        private static void Case_01948()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1948,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-18,37,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,10,68,4,3), new GeneratedEnemyUnit(-10,-12,27,42,1), new GeneratedEnemyUnit(-1,-10,7,7,1), new GeneratedEnemyUnit(-9,-7,42,44,2), new GeneratedEnemyUnit(2,-1,70,47,1), new GeneratedEnemyUnit(9,1,18,47,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 100,
                stableHash: "a02d9320a21a216a26f8a184f7979e3199a392db480ee87e7f66c590201ff40e");
        }

        private static void Case_01949()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1949,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,19,87,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,8,48,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,12,75,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-15,56,47,1), new GeneratedEnemyUnit(-7,-7,53,30,4), new GeneratedEnemyUnit(14,12,58,16,3), new GeneratedEnemyUnit(16,12,38,31,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "524db0d561b84d0b9e0ba43b771dfe77c60a95c3bbaa4a55bfb06471a07f100d");
        }

        private static void Case_01950()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1950,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-2,98,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-3,70,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-8,22,19,1), new GeneratedEnemyUnit(10,5,26,27,1), new GeneratedEnemyUnit(6,-6,45,50,2), new GeneratedEnemyUnit(1,6,14,10,4), new GeneratedEnemyUnit(2,20,5,23,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "a72eb553b10aa45a214374e987b35b72e341281e3143afce62070b482dfa585b");
        }

        private static void Case_01951()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1951,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-16,17,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-6,76,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,9,61,41,2), new GeneratedEnemyUnit(-12,-10,58,10,4), new GeneratedEnemyUnit(-3,-3,89,2,4), new GeneratedEnemyUnit(-14,20,45,32,1), new GeneratedEnemyUnit(13,-12,16,14,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "d8a289169167a1404f8a1fcc69b430395cfc7c2771cbc16b3e54e3941aa63473");
        }

        private static void Case_01952()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1952,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-17,56,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,13,100,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-5,48,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,13,39,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,2,56,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-14,29,1,3), new GeneratedEnemyUnit(-11,11,76,14,1), new GeneratedEnemyUnit(14,16,77,17,1), new GeneratedEnemyUnit(15,-5,55,49,3), new GeneratedEnemyUnit(14,-14,92,47,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "f39843ace3f27dd8f402f082952c7e3e895cbffac654a8f3fe33dee728c06fd2");
        }

        private static void Case_01953()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1953,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-12,26,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-9,68,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-2,15,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,8,78,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-10,66,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,8,87,2,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-4,47,9,1), new GeneratedEnemyUnit(-2,-9,90,49,2), new GeneratedEnemyUnit(11,7,69,1,3), new GeneratedEnemyUnit(19,-5,95,29,3), new GeneratedEnemyUnit(15,-15,5,25,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "019f93871441b5cf369c554179eb21fd4a31193dca76f68919f98cb4d8a59040");
        }

        private static void Case_01954()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1954,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-2,22,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-17,39,22,2), new GeneratedEnemyUnit(-16,-14,61,1,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "50bf97a8bcf2dd45a83883dc5b5b5542dcb7f64483463c2b972c7b18d1bcdf6a");
        }

        private static void Case_01955()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1955,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-7,6,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-14,82,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-16,22,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-4,22,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-4,81,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,8,33,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,20,19,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-16,92,14,1), new GeneratedEnemyUnit(4,-9,60,8,1), new GeneratedEnemyUnit(-15,-2,96,1,4), new GeneratedEnemyUnit(2,-5,24,42,3), new GeneratedEnemyUnit(-6,0,92,44,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "580161dd530f6a8181937909916d497ed76023f97328558fa5c52540bcccc89c");
        }

        private static void Case_01956()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1956,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-6,11,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-13,37,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-2,92,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,6,49,3,2), new GeneratedEnemyUnit(-10,-12,47,24,4), new GeneratedEnemyUnit(10,-9,22,31,2), new GeneratedEnemyUnit(4,18,48,35,4), new GeneratedEnemyUnit(-7,10,62,12,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "da9d285e759ffc3925b13fecf22ffa931bce2a55f93713ff8974cb596cc7a229");
        }

        private static void Case_01957()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1957,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-1,7,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,17,64,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-18,55,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-7,54,12,2), new GeneratedEnemyUnit(10,-17,14,1,3), new GeneratedEnemyUnit(8,16,12,6,3), new GeneratedEnemyUnit(1,-2,95,35,1), new GeneratedEnemyUnit(-16,4,9,23,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "858625c10e6537727ac11b590e5bbd246f4bbd3cb726b172bda9eaca3f3677cf");
        }

        private static void Case_01958()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1958,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,18,6,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,9,8,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-7,23,39,4), new GeneratedEnemyUnit(8,9,90,12,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "2994cf196bb343d6beaab849ec5ccd71b024ea8a8c4a52aa604d399044827541");
        }

        private static void Case_01959()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1959,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-4,65,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-16,43,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-13,14,13,3), new GeneratedEnemyUnit(1,2,16,13,4), new GeneratedEnemyUnit(19,7,75,37,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "e5beda42eb5044d28a1e8d040d9c80ca06d0780e653538e0efce9e47a5595643");
        }

        private static void Case_01960()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1960,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,5,100,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,4,63,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-18,78,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,2,94,8,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "b93ea9cf595fb23341147f0006b4f2bfa5824bf7acee891a1e025bd3eaaa46d8");
        }

        private static void Case_01961()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1961,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-15,63,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,20,66,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-2,40,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-18,91,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,12,50,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,8,92,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-12,57,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,18,85,47,2), new GeneratedEnemyUnit(-2,-14,75,27,3), new GeneratedEnemyUnit(-2,-4,45,1,3), new GeneratedEnemyUnit(19,18,12,41,1), new GeneratedEnemyUnit(-3,-8,33,47,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "429e271e62bc6e68bcf0175260aecb8b452eedcf05cf5de00233a64eac40401a");
        }

        private static void Case_01962()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1962,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-13,30,2,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,13,77,50,3), new GeneratedEnemyUnit(-12,15,83,28,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "169efd1ceb93e2b572944fe44514d8bbad1c61ad19c74d431ca32bdce2c7fee7");
        }

        private static void Case_01963()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1963,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-16,49,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,13,12,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,14,71,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,17,45,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,9,19,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,4,18,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-12,67,33,3), new GeneratedEnemyUnit(-12,14,5,18,3), new GeneratedEnemyUnit(-5,11,36,21,2), new GeneratedEnemyUnit(5,11,78,7,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "d88d641a0f82afb7bc04a259c6c5e33ab287f2fabe956e8c98fa95af058de8d6");
        }

        private static void Case_01964()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1964,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,4,42,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-9,55,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-19,75,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-1,57,38,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "3aaccbfc3ddeb5456c4c04922439f290d26a75b3702b3e98b8d410a69ec9b1bc");
        }

        private static void Case_01965()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1965,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-18,15,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,11,56,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,18,97,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,8,11,3,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "4e2d7cfd781ed4f8875a988308cc1626f832d628cb0e27ff1ae5fad3e576eb2c");
        }

        private static void Case_01966()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1966,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-12,43,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-20,66,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-9,61,4,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "bdd14d05333341e153bccfc17f168e03e98209f597f6080422ce3a40d34df9ce");
        }

        private static void Case_01967()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1967,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-11,30,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-12,59,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-12,18,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,3,57,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,20,80,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-9,81,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-19,70,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-8,27,27,3), new GeneratedEnemyUnit(18,0,30,14,4), new GeneratedEnemyUnit(-18,-14,40,46,3), new GeneratedEnemyUnit(-2,2,94,43,1), new GeneratedEnemyUnit(16,4,93,22,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "95ac8af182b43745309610694d5c8dd10416b0e53727b2c7da06702f7adb815c");
        }

        private static void Case_01968()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1968,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-12,22,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,9,97,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,1,88,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-6,35,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-20,80,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,14,74,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-2,15,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-18,29,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-15,57,15,3), new GeneratedEnemyUnit(20,-1,35,5,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "d158508c2fe2f4eec1297da4fb442311168622983844996b54280a08c134e9cf");
        }

        private static void Case_01969()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1969,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-4,66,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-13,63,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,6,51,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-8,24,48,1), new GeneratedEnemyUnit(-18,18,73,44,1), new GeneratedEnemyUnit(12,-7,64,15,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "531b1bad826092d937cd9262a8fa86ee4ec943995e505b4b3997734d078da67d");
        }

        private static void Case_01970()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1970,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,13,72,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-11,40,3,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,16,96,48,3), new GeneratedEnemyUnit(1,15,58,18,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "d18d64a72c1fb45ee28a89f786ce3c61a413a42ecc29322ec103e097bcff6bda");
        }

        private static void Case_01971()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1971,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,12,52,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,3,87,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-8,54,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-4,99,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,9,73,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,9,55,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,15,39,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,15,38,21,3), new GeneratedEnemyUnit(-7,-11,21,9,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "4c63250c7ffae110f9f0ad837470e26fa02aff4b94733795b7d347e16d0949ae");
        }

        private static void Case_01972()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1972,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,11,86,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,6,43,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-2,87,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-9,49,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,7,75,4,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,9,30,40,4), new GeneratedEnemyUnit(-14,-13,53,22,3), new GeneratedEnemyUnit(-5,-2,76,2,2), new GeneratedEnemyUnit(17,-7,13,20,3), new GeneratedEnemyUnit(19,-14,24,47,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 45,
                stableHash: "916a6acc3983ffb72eb7dd75e15a0910f2139fbf81071d8529609ad3edc73a54");
        }

        private static void Case_01973()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1973,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-13,12,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-11,93,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,10,65,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-12,64,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,11,80,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-3,71,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-12,90,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-7,90,39,1), new GeneratedEnemyUnit(-14,3,41,45,1), new GeneratedEnemyUnit(-14,-9,64,1,1), new GeneratedEnemyUnit(-7,14,89,46,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "ea2a0a964fe8c13b0f4da7fc07bd02b76b55478b17123198ef6afd27a18f1424");
        }

        private static void Case_01974()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1974,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-4,16,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-20,47,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-17,25,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,13,74,2,1,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "2c62a42c9c108d4aaa28d1937276480f742658d97a3e61a8ef0b2d999906d5a1");
        }

        private static void Case_01975()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1975,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,4,91,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,3,42,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,13,17,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-2,51,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,0,80,9,4), new GeneratedEnemyUnit(9,19,43,25,3), new GeneratedEnemyUnit(9,-9,92,24,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "907c8aa97fd654fdb41bef72849abe62ab954c92ca7f84a49027e7571cda53f4");
        }

        private static void Case_01976()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1976,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,8,69,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-17,67,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-10,79,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,6,34,1,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 15,
                stableHash: "a705c40b524cc76e7d4eca2873972423038c871d108501ad3e3b9c23ce9f6da0");
        }

        private static void Case_01977()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1977,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,7,22,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,5,31,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,2,24,46,3), new GeneratedEnemyUnit(-5,-5,6,49,4), new GeneratedEnemyUnit(16,7,25,11,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "706e33591b5e1e3b39b71c05021ab7047fef02360aaaf72cafc81bca82426c5b");
        }

        private static void Case_01978()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1978,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-18,41,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,9,96,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,14,47,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,5,23,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-19,45,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-4,77,4,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "331f025c0f6f6fe7fe10ff81951b1eda6fee7edc2c2d774922a0c83881185ef9");
        }

        private static void Case_01979()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 1979,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,2,77,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-4,34,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-11,42,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,5,26,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,5,5,23,2), new GeneratedEnemyUnit(19,13,46,13,2), new GeneratedEnemyUnit(-16,4,69,2,4), new GeneratedEnemyUnit(-18,-18,77,19,3), new GeneratedEnemyUnit(-19,11,31,14,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "f246fc14aee33a2061ed0220447ff78c7ca68d7064b6ac802a0ff7deba75c8fd");
        }

    }
}
