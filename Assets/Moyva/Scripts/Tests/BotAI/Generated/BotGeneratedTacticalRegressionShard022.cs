using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard022
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_03960();
            Case_03961();
            Case_03962();
            Case_03963();
            Case_03964();
            Case_03965();
            Case_03966();
            Case_03967();
            Case_03968();
            Case_03969();
            Case_03970();
            Case_03971();
            Case_03972();
            Case_03973();
            Case_03974();
            Case_03975();
            Case_03976();
            Case_03977();
            Case_03978();
            Case_03979();
            Case_03980();
            Case_03981();
            Case_03982();
            Case_03983();
            Case_03984();
            Case_03985();
            Case_03986();
            Case_03987();
            Case_03988();
            Case_03989();
            Case_03990();
            Case_03991();
            Case_03992();
            Case_03993();
            Case_03994();
            Case_03995();
            Case_03996();
            Case_03997();
            Case_03998();
            Case_03999();
            Case_04000();
            Case_04001();
            Case_04002();
            Case_04003();
            Case_04004();
            Case_04005();
            Case_04006();
            Case_04007();
            Case_04008();
            Case_04009();
            Case_04010();
            Case_04011();
            Case_04012();
            Case_04013();
            Case_04014();
            Case_04015();
            Case_04016();
            Case_04017();
            Case_04018();
            Case_04019();
            Case_04020();
            Case_04021();
            Case_04022();
            Case_04023();
            Case_04024();
            Case_04025();
            Case_04026();
            Case_04027();
            Case_04028();
            Case_04029();
            Case_04030();
            Case_04031();
            Case_04032();
            Case_04033();
            Case_04034();
            Case_04035();
            Case_04036();
            Case_04037();
            Case_04038();
            Case_04039();
            Case_04040();
            Case_04041();
            Case_04042();
            Case_04043();
            Case_04044();
            Case_04045();
            Case_04046();
            Case_04047();
            Case_04048();
            Case_04049();
            Case_04050();
            Case_04051();
            Case_04052();
            Case_04053();
            Case_04054();
            Case_04055();
            Case_04056();
            Case_04057();
            Case_04058();
            Case_04059();
            Case_04060();
            Case_04061();
            Case_04062();
            Case_04063();
            Case_04064();
            Case_04065();
            Case_04066();
            Case_04067();
            Case_04068();
            Case_04069();
            Case_04070();
            Case_04071();
            Case_04072();
            Case_04073();
            Case_04074();
            Case_04075();
            Case_04076();
            Case_04077();
            Case_04078();
            Case_04079();
            Case_04080();
            Case_04081();
            Case_04082();
            Case_04083();
            Case_04084();
            Case_04085();
            Case_04086();
            Case_04087();
            Case_04088();
            Case_04089();
            Case_04090();
            Case_04091();
            Case_04092();
            Case_04093();
            Case_04094();
            Case_04095();
            Case_04096();
            Case_04097();
            Case_04098();
            Case_04099();
            Case_04100();
            Case_04101();
            Case_04102();
            Case_04103();
            Case_04104();
            Case_04105();
            Case_04106();
            Case_04107();
            Case_04108();
            Case_04109();
            Case_04110();
            Case_04111();
            Case_04112();
            Case_04113();
            Case_04114();
            Case_04115();
            Case_04116();
            Case_04117();
            Case_04118();
            Case_04119();
            Case_04120();
            Case_04121();
            Case_04122();
            Case_04123();
            Case_04124();
            Case_04125();
            Case_04126();
            Case_04127();
            Case_04128();
            Case_04129();
            Case_04130();
            Case_04131();
            Case_04132();
            Case_04133();
            Case_04134();
            Case_04135();
            Case_04136();
            Case_04137();
            Case_04138();
            Case_04139();
        }

        private static void Case_03960()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3960,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,11,52,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-6,38,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-4,49,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,15,99,23,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "fb19be8b0e9c28f3d69fd22b45a847bacfb18625eff3537e420e1ad3ae51a300");
        }

        private static void Case_03961()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3961,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,0,76,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,18,12,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,8,54,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,10,40,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-15,35,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,14,89,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-20,14,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,19,41,3,3), new GeneratedEnemyUnit(1,-6,22,17,2), new GeneratedEnemyUnit(-16,16,9,9,4), new GeneratedEnemyUnit(17,17,78,46,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "c6f8f781c5f4342d30e8c7ca6ef5e45e4e248f412040a5f1c2cbb425155203bc");
        }

        private static void Case_03962()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3962,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-20,13,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,20,50,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,10,25,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,17,64,27,3), new GeneratedEnemyUnit(18,-20,90,22,3), new GeneratedEnemyUnit(13,17,36,10,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "ac0ff11f9920e817272a61cb121f795e20e7d1529c7dd65ebd273e16d74aa3f7");
        }

        private static void Case_03963()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3963,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-9,42,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,11,19,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,5,42,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-1,63,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-15,22,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,3,63,24,4), new GeneratedEnemyUnit(15,-2,70,44,4), new GeneratedEnemyUnit(13,-20,7,30,2), new GeneratedEnemyUnit(1,-8,28,6,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "bfb41775108242a84efbf4cc128acca2b4ea6984c4a8615c72a348b2e6069854");
        }

        private static void Case_03964()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3964,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-6,79,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,19,34,24,4), new GeneratedEnemyUnit(-4,11,94,39,4), new GeneratedEnemyUnit(20,-16,89,25,3), new GeneratedEnemyUnit(-20,-6,41,15,1), new GeneratedEnemyUnit(20,14,93,4,4), new GeneratedEnemyUnit(8,-3,60,13,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 100,
                stableHash: "3160d6df8431ab19e555dba9fb1af15c9cb9089b25a134da1fc813be1df89643");
        }

        private static void Case_03965()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3965,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,8,86,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-11,94,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-19,73,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-3,52,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,17,53,3,1), new GeneratedEnemyUnit(-11,-6,57,9,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "bb0859d8e776a1a52b81e712bfe52303cc713de59f171cb001ff6c76502773a1");
        }

        private static void Case_03966()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3966,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,2,14,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-3,59,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-2,94,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-5,42,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,6,29,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-17,39,6,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "c135a6ad1c3b7a00e9d60e728ee4d7c6e46942c58ce404c62da0693bc6e54019");
        }

        private static void Case_03967()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3967,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,7,32,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-11,5,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,2,62,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-2,29,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,10,68,3,3), new GeneratedEnemyUnit(-1,-1,13,49,4), new GeneratedEnemyUnit(1,-18,65,13,2), new GeneratedEnemyUnit(16,-7,66,31,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "c186d721641b421916aa76a900eb6ba9fa91e31959f8e609fdc5c6571b5fd6ba");
        }

        private static void Case_03968()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3968,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-8,67,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,13,47,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-2,26,43,1), new GeneratedEnemyUnit(-9,-12,99,36,4), new GeneratedEnemyUnit(20,-10,15,17,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "bc38c919d10d3dd25b75d362754457de8ac798fe50b6fc5b502b67344f378e07");
        }

        private static void Case_03969()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3969,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-9,92,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,15,54,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,1,82,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-1,24,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,11,69,22,3), new GeneratedEnemyUnit(-10,-7,59,4,3), new GeneratedEnemyUnit(4,12,53,45,3), new GeneratedEnemyUnit(3,8,88,11,2), new GeneratedEnemyUnit(14,-16,22,1,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "24d54cc1f140fdfafff1a7e03e2ca554977f4603ce8721bc6d72726fd444f84f");
        }

        private static void Case_03970()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3970,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-13,28,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,12,22,12,3), new GeneratedEnemyUnit(14,0,83,8,4), new GeneratedEnemyUnit(-14,10,87,42,1), new GeneratedEnemyUnit(-9,17,100,8,1), new GeneratedEnemyUnit(-3,12,75,21,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "eb129de44386bde9725c97ee199463a0ef372660a910f57056f8b405831693bd");
        }

        private static void Case_03971()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3971,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,10,79,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,3,78,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,11,33,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-5,5,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-14,99,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-17,58,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,15,81,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,4,26,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,12,31,21,4), new GeneratedEnemyUnit(3,-19,95,9,2), new GeneratedEnemyUnit(-12,7,10,4,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "482f9ca1f3459b89fd0e95236b5e91f22eef048138f3f1b8a29b739f9443c142");
        }

        private static void Case_03972()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3972,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-3,93,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-20,27,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,2,62,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-2,24,6,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "b3032c48bbdbfd45536f81efbbea3befeeb13ff9ef0a0bdfd733bcee2ce3d479");
        }

        private static void Case_03973()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3973,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,7,80,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-10,65,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,11,7,20,3), new GeneratedEnemyUnit(18,2,66,19,3), new GeneratedEnemyUnit(-18,-20,22,13,4), new GeneratedEnemyUnit(-14,3,34,33,1), new GeneratedEnemyUnit(-2,-3,98,4,2), new GeneratedEnemyUnit(-7,9,40,33,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "26950aa732d8a7637c1d89b93a72ec324fbd3a2d20f2e38e2d8bfef6b3e76bfa");
        }

        private static void Case_03974()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3974,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-5,69,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-16,21,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-10,63,30,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "f6df453046bb0087c9b2db321c70aebedb8e408446cf2dd000ac16862ef4024e");
        }

        private static void Case_03975()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3975,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,-13,67,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-1,90,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-9,93,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,2,34,10,1), new GeneratedEnemyUnit(19,-1,17,22,1), new GeneratedEnemyUnit(16,-16,88,38,4), new GeneratedEnemyUnit(1,-17,12,31,3), new GeneratedEnemyUnit(1,-5,77,2,3), new GeneratedEnemyUnit(-5,10,44,40,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "9b41d9caad964aa6a410df1e5404940d3da6a6bb3f5e9485c701f3fe0f8035b3");
        }

        private static void Case_03976()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3976,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,5,14,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,20,22,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-20,80,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-13,23,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-15,69,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-7,11,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,19,73,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-7,12,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,19,12,28,1), new GeneratedEnemyUnit(12,17,91,42,4), new GeneratedEnemyUnit(-11,3,53,41,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "5921b9672236ef6bfe5a14c04a1e4201710a0227eb9510bb53131c81737fe637");
        }

        private static void Case_03977()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3977,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-20,18,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-6,63,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,-18,35,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,19,18,43,3), new GeneratedEnemyUnit(-4,-1,11,2,1), new GeneratedEnemyUnit(-12,10,10,9,2), new GeneratedEnemyUnit(-11,-19,92,27,4), new GeneratedEnemyUnit(-19,-3,89,27,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "b23e5637256ddefdd008f06cace75b563f26f4b97f9bc509478fde59c854f594");
        }

        private static void Case_03978()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3978,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,17,72,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,-20,30,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-3,94,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-10,6,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,1,5,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-7,32,3,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,6,13,47,1), new GeneratedEnemyUnit(4,0,84,19,2), new GeneratedEnemyUnit(-2,6,80,31,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "8d800c6e3ebe345eda40f601884e8af844c94075174ebcf72cb176391f7c2a76");
        }

        private static void Case_03979()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3979,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,11,54,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,10,60,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-16,94,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,4,100,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-19,41,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-7,58,49,2), new GeneratedEnemyUnit(19,18,84,17,4), new GeneratedEnemyUnit(-13,20,56,44,4), new GeneratedEnemyUnit(-9,-12,38,32,3), new GeneratedEnemyUnit(-17,17,60,28,3), new GeneratedEnemyUnit(-6,-20,52,39,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "d93aba10120c209bd31d17da4923147294394b96d3fe2a71fb726af70f1698d9");
        }

        private static void Case_03980()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3980,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,19,93,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-15,46,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,16,12,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-7,85,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,0,11,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-17,10,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,4,99,32,1), new GeneratedEnemyUnit(19,10,43,12,4), new GeneratedEnemyUnit(-18,-13,18,10,1), new GeneratedEnemyUnit(-1,-12,19,39,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "944f83f3fe4ab1ec6e8c8e307f29dfd3b5e8027429d3e7fa163265f1cd601a67");
        }

        private static void Case_03981()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3981,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,7,18,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-2,34,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-4,82,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-18,27,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-19,79,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,0,51,10,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "fc90a7a1bf57be061530a6f68c7c0daa8721a64302704e24620a37e7b9da5b37");
        }

        private static void Case_03982()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3982,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-18,55,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,5,31,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,9,79,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-7,7,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-20,8,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-8,50,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-9,55,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,3,29,2,1), new GeneratedEnemyUnit(-9,5,42,12,2), new GeneratedEnemyUnit(13,-12,68,6,4), new GeneratedEnemyUnit(6,-5,41,16,4), new GeneratedEnemyUnit(16,-12,7,33,3), new GeneratedEnemyUnit(12,-13,60,48,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "59afa0302ffdf9a0a1ff127e8933cb1d3e672fa03803dfefb19cb8fc17e5268d");
        }

        private static void Case_03983()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3983,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-15,9,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-19,55,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-3,38,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,2,90,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-12,62,40,3), new GeneratedEnemyUnit(-4,0,5,15,3), new GeneratedEnemyUnit(14,-16,36,27,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "0e57478956bd8839b6dfd936ce95b80ac718af60416afc28800bd5435b044175");
        }

        private static void Case_03984()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3984,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,9,64,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,12,49,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-7,18,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-3,79,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-14,55,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,6,11,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,0,55,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,4,82,6,2), new GeneratedEnemyUnit(4,-8,67,41,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "f5a7276f6029f2296a15a65b1d2df73ce72c255a4da41a65ba7a34880336ff59");
        }

        private static void Case_03985()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3985,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-15,70,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-20,43,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-10,77,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-2,46,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-4,45,22,4), new GeneratedEnemyUnit(16,-2,45,47,1), new GeneratedEnemyUnit(-6,20,60,46,2), new GeneratedEnemyUnit(-7,1,26,8,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "0479062bd6bf9265d92b83a0530b0ca94a3812725a41ded87fcbe95d17d88866");
        }

        private static void Case_03986()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3986,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,12,96,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,3,54,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,7,90,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,15,97,25,3), new GeneratedEnemyUnit(-13,15,34,17,2), new GeneratedEnemyUnit(-18,-3,17,26,3), new GeneratedEnemyUnit(16,1,69,9,4), new GeneratedEnemyUnit(11,10,90,5,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "fa7a22dee01b820cb52339b5752d657317f44bf01c00ac923c7c02d2c4b945a9");
        }

        private static void Case_03987()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3987,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-16,89,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-18,94,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-20,71,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-11,52,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-18,18,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-18,21,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,3,77,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-6,91,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-14,58,32,2), new GeneratedEnemyUnit(-16,-9,72,11,4), new GeneratedEnemyUnit(-7,2,14,30,4), new GeneratedEnemyUnit(9,-19,10,49,2), new GeneratedEnemyUnit(-6,4,77,31,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "bee0392c9f0800a54c854ff6b3d942e653cbc6864ffdf4937d5d6e003edeb23a");
        }

        private static void Case_03988()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3988,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-10,66,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,15,98,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,15,26,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,2,74,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-5,45,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-5,7,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,5,94,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-8,98,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,8,68,17,3), new GeneratedEnemyUnit(0,-3,51,24,2), new GeneratedEnemyUnit(9,13,37,47,2), new GeneratedEnemyUnit(12,5,39,12,1), new GeneratedEnemyUnit(10,-20,59,36,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "905597e97eae33cd1dc4d3e8405d60b489d6c435a12c6e5722378ef1a8927bd4");
        }

        private static void Case_03989()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3989,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,17,15,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-5,22,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-12,16,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-12,42,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,-8,80,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,14,75,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-2,94,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,16,79,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,2,49,42,2), new GeneratedEnemyUnit(2,8,9,35,1), new GeneratedEnemyUnit(15,3,45,13,2), new GeneratedEnemyUnit(-19,18,63,48,4), new GeneratedEnemyUnit(-13,-5,73,20,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "264e32d2656eee2b5f00b6023786d26c776ffc54926533f3039246d4284f86a9");
        }

        private static void Case_03990()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3990,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-11,63,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,-17,90,29,2), new GeneratedEnemyUnit(-11,6,49,38,2), new GeneratedEnemyUnit(-20,-10,11,13,4), new GeneratedEnemyUnit(1,-4,51,13,4), new GeneratedEnemyUnit(-16,9,80,31,3), new GeneratedEnemyUnit(-15,20,29,22,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "13b147ea597a45887ecc09bbebb8b210a985dac7f81e35d1c484069ed12b4e1f");
        }

        private static void Case_03991()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3991,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,-3,21,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,15,36,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,17,97,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-3,22,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-16,85,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,2,28,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,19,79,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-4,36,3,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,1,58,49,4), new GeneratedEnemyUnit(17,-18,17,40,2), new GeneratedEnemyUnit(-14,20,10,22,1), new GeneratedEnemyUnit(3,-16,15,21,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "e35c749c4ce475aa9521a26631335e9062319630e90e5cee268479af0b8ae383");
        }

        private static void Case_03992()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3992,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-17,73,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-16,50,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-5,61,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,6,83,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,6,90,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-15,26,1,4,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "407dec5dc194de1be0350be2921e098bda59bcbcf66845d493784bf108217c3a");
        }

        private static void Case_03993()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3993,
                posture: BotStrategicPosture.Search,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-8,44,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-11,63,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,13,71,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-4,83,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-20,74,1,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-2,66,46,2), new GeneratedEnemyUnit(2,19,65,50,3), new GeneratedEnemyUnit(-15,-2,9,29,2), new GeneratedEnemyUnit(-6,-4,42,8,3), new GeneratedEnemyUnit(-6,-11,48,29,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "53986e8d95e722fdd6bf8bf0fad5e1ad17b78af021e34ce9853ddccdfb634a2e");
        }

        private static void Case_03994()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3994,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-12,13,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,3,31,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,-16,24,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,5,38,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-12,31,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,6,67,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,17,95,37,1), new GeneratedEnemyUnit(1,-4,59,46,3), new GeneratedEnemyUnit(-3,19,85,1,3), new GeneratedEnemyUnit(14,-20,27,32,2), new GeneratedEnemyUnit(-3,-15,69,31,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "5aa65578e27f3be84dac363f36712cff764389a0780d9dafe70b17e78fff470f");
        }

        private static void Case_03995()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3995,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,20,89,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,12,50,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-19,21,4,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,18,7,3,3), new GeneratedEnemyUnit(-13,5,8,12,3), new GeneratedEnemyUnit(8,-12,94,12,1), new GeneratedEnemyUnit(1,-17,55,24,1), new GeneratedEnemyUnit(12,7,38,25,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "7e0c104da3ee5f83b990eea2fac5ad294b96a9809f3df1cc5e0b082b014f8348");
        }

        private static void Case_03996()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3996,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,-10,82,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,20,38,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-2,80,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-7,93,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-17,33,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,11,81,4,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "076c5e31ca3a6fe77f1cce6c5c0963ca8c911f80e413ce0ae76af94dd33db53f");
        }

        private static void Case_03997()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3997,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,14,33,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,1,48,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,0,41,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-13,17,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-19,64,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,12,69,7,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "f382807877b08ee4562d964ee9ff69cdf5079c0f70f974210607a62a2949c96c");
        }

        private static void Case_03998()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3998,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-7,28,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,7,48,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,7,41,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-11,41,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-7,96,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,9,82,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-2,50,3,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-20,18,23,4), new GeneratedEnemyUnit(19,2,84,7,1), new GeneratedEnemyUnit(-20,-12,96,18,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "71f6bb543becb66ef9ff0ca63f6db6b49aeb6e0fb2450d5f2284e55088a9d515");
        }

        private static void Case_03999()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 3999,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,14,73,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,2,65,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-1,97,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-5,12,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,15,61,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,-2,66,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,15,50,12,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "f8a3fdb7f830e7375441ebcbecc3bd2704605ccbab0a9effb4d18d075c25b0b5");
        }

        private static void Case_04000()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4000,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,6,45,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,6,27,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-16,12,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-18,60,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,15,51,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,8,18,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,-6,55,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,15,6,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-2,17,18,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "3820d33589c7405ef8307c85987293515bb32daf39909e75fe42d7ea8279dbf3");
        }

        private static void Case_04001()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4001,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-4,53,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,7,100,3,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-10,10,41,1), new GeneratedEnemyUnit(11,-13,80,27,3), new GeneratedEnemyUnit(-3,-15,92,14,2), new GeneratedEnemyUnit(-7,-17,75,34,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "8bd67e2d0785275a163c8871d4470471f3a94464be7a98985ecd133ed86ba238");
        }

        private static void Case_04002()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4002,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,6,54,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,7,33,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,5,94,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-5,89,30,1), new GeneratedEnemyUnit(3,-17,16,34,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "ee7e3105e4d5bf84669cdc6c779daee1c7f06888f6977985a6ac3d89f1f262c8");
        }

        private static void Case_04003()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4003,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,20,21,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-19,75,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,8,45,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,2,89,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,1,20,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,16,99,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-5,93,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-20,90,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-16,95,35,4), new GeneratedEnemyUnit(-15,7,48,8,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "a8f5123376a7e9ee65d5ae01611e8a35c9d0513000f52be268fce72efd577b68");
        }

        private static void Case_04004()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4004,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,0,50,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,11,19,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,16,27,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-10,50,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,15,16,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,19,53,43,1), new GeneratedEnemyUnit(-3,9,92,28,1), new GeneratedEnemyUnit(-3,7,10,5,2), new GeneratedEnemyUnit(-20,-8,22,33,3), new GeneratedEnemyUnit(19,-1,46,48,4), new GeneratedEnemyUnit(7,-8,82,23,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "04360ac7547abd1e152572f798b0603db96af9906121ee1ea89e0fcdd47668ba");
        }

        private static void Case_04005()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4005,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,14,54,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,18,34,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-1,100,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-11,19,3,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-15,22,38,3), new GeneratedEnemyUnit(1,15,51,16,3), new GeneratedEnemyUnit(-1,12,59,47,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "adf16ede42a2f7bc923f7ebaec07f1c635c9435597e46d34e57b2d2e0b19a87c");
        }

        private static void Case_04006()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4006,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-14,64,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-11,5,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-11,68,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,11,5,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-15,15,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,14,56,7,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "a176e8ebe4afaf133cb6b90928f2ccd5cfff6e566c0f5b08bd5fbe279a7ccfa5");
        }

        private static void Case_04007()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4007,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-9,71,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-13,79,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,16,97,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,7,19,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,11,26,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-20,28,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,2,41,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,19,14,5,3,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "d5be031e22d20edba807efc3ddfd2151cc588d3fc13811caf45793930bc0d12c");
        }

        private static void Case_04008()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4008,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-2,57,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,16,34,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,2,40,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,12,30,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,16,71,25,2), new GeneratedEnemyUnit(-14,18,46,18,4), new GeneratedEnemyUnit(4,5,17,3,1), new GeneratedEnemyUnit(-9,-1,90,2,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "88cdf35e98ac7a046b4659828029a09b35f2988dc53911057b98f436d94d2619");
        }

        private static void Case_04009()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4009,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,13,62,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-7,22,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,18,78,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-8,72,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-7,34,21,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "7218f3cfad17dfde580426dae75181c50ca794928da0326d7b245029db8444c7");
        }

        private static void Case_04010()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4010,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-7,25,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-3,48,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-5,33,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-6,69,7,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-17,58,41,4), new GeneratedEnemyUnit(3,-14,15,41,2), new GeneratedEnemyUnit(-10,-11,71,36,4), new GeneratedEnemyUnit(-12,-17,18,32,2), new GeneratedEnemyUnit(-11,-17,36,45,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "0b39267b51bcdcf81f80bc734da6ad5a76f2ca9427d740eaa70830cbbb106eab");
        }

        private static void Case_04011()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4011,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,8,97,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,0,10,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,19,90,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,10,47,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,3,28,1,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,15,67,20,2), new GeneratedEnemyUnit(16,-18,56,38,4), new GeneratedEnemyUnit(9,-14,98,40,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "7766c69d167a94804cc885b460a4543cb882a3ef9003385fe461a03de6773215");
        }

        private static void Case_04012()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4012,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-14,31,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-5,24,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-8,53,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,20,5,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,18,33,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,13,43,6,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "3e937b4afba8bd04a6b2b4878395aad87b6c1794b5871b2b379b1db6926fc59d");
        }

        private static void Case_04013()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4013,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-3,14,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-15,64,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-20,92,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,19,96,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-15,31,28,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "0033f191576bb9e92c094c11393752460c1c46e90660fd29a04fa0a39c096625");
        }

        private static void Case_04014()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4014,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,14,39,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-14,23,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,20,79,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,-16,80,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,3,5,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-5,16,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,3,36,17,3), new GeneratedEnemyUnit(8,-8,29,43,1), new GeneratedEnemyUnit(-14,1,84,37,4), new GeneratedEnemyUnit(-6,8,28,50,2), new GeneratedEnemyUnit(17,19,12,26,3), new GeneratedEnemyUnit(6,17,57,38,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "d7b828c20c10988f55f164acb114c1f399de6e7792b8f4e76877976a26db6214");
        }

        private static void Case_04015()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4015,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-17,74,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,11,23,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,6,91,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,18,18,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-11,56,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,6,80,14,4), new GeneratedEnemyUnit(5,-8,84,45,2), new GeneratedEnemyUnit(-11,-8,94,9,1), new GeneratedEnemyUnit(-7,4,55,7,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "e9c8add152110c3a86722af09f8bc2bdf9ee3e0c7936c9dc5d8c228027d4139f");
        }

        private static void Case_04016()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4016,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,17,44,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-19,17,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,2,56,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,2,43,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-17,31,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,7,20,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-4,47,21,4), new GeneratedEnemyUnit(-13,5,5,48,4), new GeneratedEnemyUnit(1,18,84,4,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "fd2e1b74de803018033c9da62f4763fb833b600ec1c47fcccf20342a8c44da19");
        }

        private static void Case_04017()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4017,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,18,61,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-9,89,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,8,73,5,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-10,56,2,3), new GeneratedEnemyUnit(-20,-5,21,21,4), new GeneratedEnemyUnit(-9,4,41,4,1), new GeneratedEnemyUnit(12,-10,20,8,2), new GeneratedEnemyUnit(-16,-20,92,3,4), new GeneratedEnemyUnit(11,-15,23,44,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 55,
                stableHash: "8eeb79b2067d6156e9b57cf74621ef9ae80fd168a0a6afa0f8b3043d38623384");
        }

        private static void Case_04018()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4018,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-1,36,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-15,31,2,2), new GeneratedEnemyUnit(7,-11,77,44,3), new GeneratedEnemyUnit(-9,20,49,39,1), new GeneratedEnemyUnit(2,-4,80,27,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "259e46cc6b946aa2ac30c4a9bbfc833081a7f5b68b45c6d8c0c04b9d87d59c52");
        }

        private static void Case_04019()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4019,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,16,69,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,4,84,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,8,100,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-13,39,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-3,69,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-2,48,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,1,56,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,7,82,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-19,29,50,4), new GeneratedEnemyUnit(-7,12,13,13,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "663d934c2c0d31160525f9a3d25fca744fd90421e31153b4a8d7e591bb3ed703");
        }

        private static void Case_04020()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4020,
                posture: BotStrategicPosture.Search,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-16,100,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,15,56,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,16,48,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,7,25,1,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "557e985c21bda16b94c9ee348e3656929fde41f507641e539f3cc329a6a5fb6d");
        }

        private static void Case_04021()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4021,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,4,63,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-15,29,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-7,24,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-16,83,23,2), new GeneratedEnemyUnit(-19,10,44,31,4), new GeneratedEnemyUnit(7,-5,19,17,1), new GeneratedEnemyUnit(17,-6,9,10,4), new GeneratedEnemyUnit(-3,-5,47,38,2), new GeneratedEnemyUnit(1,8,53,38,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "bd34b22c446e293b50db5d9278297de36b61921bbd6b085be5b08f936be069ad");
        }

        private static void Case_04022()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4022,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,14,66,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-15,40,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,15,29,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,1,74,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-19,51,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,18,20,44,1), new GeneratedEnemyUnit(7,-1,55,47,3), new GeneratedEnemyUnit(-14,19,83,36,2), new GeneratedEnemyUnit(-11,9,77,1,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "8107c14e84eeb204641989d7577042f44bc45a3c138599595ff37bdb5abf1382");
        }

        private static void Case_04023()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4023,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,5,58,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-13,66,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,11,34,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-13,76,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-1,75,47,3), new GeneratedEnemyUnit(-5,-3,100,18,4), new GeneratedEnemyUnit(-8,-2,89,32,4), new GeneratedEnemyUnit(17,16,69,44,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "520688828bfa2c53e13ad243c0f7c1d10e0865feda52b7659999d1bf24f4bb18");
        }

        private static void Case_04024()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4024,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-7,34,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,7,55,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,8,40,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,5,42,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-3,17,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,10,54,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,18,65,2,3), new GeneratedEnemyUnit(-4,-10,71,22,1), new GeneratedEnemyUnit(-11,-10,37,15,2), new GeneratedEnemyUnit(-12,-2,90,8,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "105853fe4011da10d1525de1e8efcd7a3e54b43beb18fe9d5d075b31bacb8572");
        }

        private static void Case_04025()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4025,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-7,16,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,17,45,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-16,93,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-18,88,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-19,69,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-11,23,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-18,6,26,4), new GeneratedEnemyUnit(-1,15,96,6,2), new GeneratedEnemyUnit(-19,12,99,2,1), new GeneratedEnemyUnit(1,20,49,1,3), new GeneratedEnemyUnit(19,-16,79,2,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "a5a941d2e55a276239f2908996f9074952c12e334f4b4725b5c0208214abf4d7");
        }

        private static void Case_04026()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4026,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-8,44,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,19,80,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,6,51,3,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-20,14,4,4), new GeneratedEnemyUnit(-1,-11,31,50,3), new GeneratedEnemyUnit(7,8,83,20,2), new GeneratedEnemyUnit(7,14,70,2,1), new GeneratedEnemyUnit(4,-2,76,25,3), new GeneratedEnemyUnit(-13,14,23,5,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "7912d6f740dedad0200071f0523ad4517d429c927821f89b4d6ceb8011629446");
        }

        private static void Case_04027()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4027,
                posture: BotStrategicPosture.Siege,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,20,51,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-1,37,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,11,28,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-13,59,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-14,9,32,2), new GeneratedEnemyUnit(7,7,36,49,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "8c3759e20f4074d7623906f1d518932b411123ba7fcee65a20fb18df6d75d493");
        }

        private static void Case_04028()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4028,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-19,53,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-15,53,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,13,91,40,4), new GeneratedEnemyUnit(19,7,30,26,2), new GeneratedEnemyUnit(-17,16,71,12,3), new GeneratedEnemyUnit(11,18,70,27,3), new GeneratedEnemyUnit(-15,12,83,42,4), new GeneratedEnemyUnit(-9,1,52,32,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "be193ada7a78140b6b306e3dd9a60e90138412df97107e58473d897d447646ca");
        }

        private static void Case_04029()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4029,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,7,34,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-20,93,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,19,93,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,7,25,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-7,20,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-3,11,37,3), new GeneratedEnemyUnit(11,-17,29,42,2), new GeneratedEnemyUnit(-6,1,41,38,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "ebdfd691df7f24f7d789c023ff9a0d42c1ada1f08462a9134118256064ebfc79");
        }

        private static void Case_04030()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4030,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,0,67,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-11,75,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,17,27,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,2,99,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-16,5,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,15,47,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-15,31,20,3), new GeneratedEnemyUnit(1,-2,43,46,1), new GeneratedEnemyUnit(-14,16,99,9,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "ae221956db54aca3ebad2f1a4188ce637f4c46a29290fd12a2d414c3d6d347b8");
        }

        private static void Case_04031()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4031,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-6,35,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,0,15,24,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "af8aa443141d09211927e523ce6268e2e84f8c95501e289a07154fa7f727ec18");
        }

        private static void Case_04032()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4032,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-15,25,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,8,22,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,8,85,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-15,18,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,11,8,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-12,50,7,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-1,31,10,2), new GeneratedEnemyUnit(-14,-18,97,39,2), new GeneratedEnemyUnit(15,-2,27,1,4), new GeneratedEnemyUnit(11,19,28,1,3), new GeneratedEnemyUnit(-20,-18,77,3,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "09f348a7236aa68fb1a7a153efa1c32b2de8347f85034c9cfdcfa07ab5888963");
        }

        private static void Case_04033()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4033,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,9,44,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-20,10,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-6,30,5,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "dd373519de75e246307f8d137528f2126625fc0fb38e718b32f35fade49082c6");
        }

        private static void Case_04034()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4034,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-13,46,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-18,23,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-9,80,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-7,99,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-15,81,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,19,31,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,18,97,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-9,79,11,3), new GeneratedEnemyUnit(4,17,32,17,2), new GeneratedEnemyUnit(16,18,60,21,4), new GeneratedEnemyUnit(19,0,9,18,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "3256a958aba3ec77603de7a0c22d188b3a6a6c53b246ab5ee1678ae7097870eb");
        }

        private static void Case_04035()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4035,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-15,80,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-14,87,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-20,77,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-16,94,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-9,36,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,12,60,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-14,68,19,4), new GeneratedEnemyUnit(-16,20,38,23,2), new GeneratedEnemyUnit(9,12,44,8,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "0977bb23987c7b7638624e0ff26b82dd228523d1fe035bf0a8383557c5db863f");
        }

        private static void Case_04036()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4036,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,14,53,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,-10,51,4,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "31e11cc909e644c74b565ef2c9c0df0708a4cbad9c7c3d9188af1344aa7890b5");
        }

        private static void Case_04037()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4037,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,12,80,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-2,48,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-12,59,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-6,69,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-15,37,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-14,27,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,5,55,6,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "acd6d9d20d6b864744f4ee02d8d8e38ee9a52ef2a7837d4089b3c8b0fdeb1859");
        }

        private static void Case_04038()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4038,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,12,50,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,5,61,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-11,44,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,10,69,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-15,33,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-6,39,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-5,88,3,1), new GeneratedEnemyUnit(9,-15,30,4,3), new GeneratedEnemyUnit(19,8,14,26,2), new GeneratedEnemyUnit(13,-19,41,4,3), new GeneratedEnemyUnit(-6,-17,39,33,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "f743107ea590b15660747a478c81054f328b666918ec22acede649012d1b1c95");
        }

        private static void Case_04039()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4039,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-2,75,2,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-19,46,27,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "c02d5bf13372c4b36f2e45ce48234634c95d4164d2cf2a2395bd4c964c83a9e7");
        }

        private static void Case_04040()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4040,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-11,39,2,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-4,96,34,4), new GeneratedEnemyUnit(-2,-20,67,49,3), new GeneratedEnemyUnit(-5,2,42,23,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "43e06e3d6452e4096613d748562a4302e7e74e55c8b197a64f8b255bb6dc0788");
        }

        private static void Case_04041()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4041,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,2,55,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,8,50,4,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-15,49,6,2), new GeneratedEnemyUnit(-15,13,15,16,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "b98243c1e849890a9ed694f7d2655fb0af48d7f9c1c6366c25ccc05c570087eb");
        }

        private static void Case_04042()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4042,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,13,39,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,0,66,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,20,86,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,11,50,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-7,34,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,9,72,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-17,76,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-7,79,7,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,10,35,12,3), new GeneratedEnemyUnit(2,-2,16,7,1), new GeneratedEnemyUnit(-12,-5,73,37,4), new GeneratedEnemyUnit(-3,-4,78,35,4), new GeneratedEnemyUnit(-13,-19,34,50,2), new GeneratedEnemyUnit(-1,17,56,45,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 55,
                stableHash: "8f28568e75953dd09e6ffc0984bc27447c5f1a16daa9f33aacc269240a4c23b2");
        }

        private static void Case_04043()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4043,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-10,11,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,5,68,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,3,50,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-18,15,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,5,54,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,16,36,17,4), new GeneratedEnemyUnit(19,-6,78,19,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "567905c1dd9645e870d204465016d242915b1f4731d0c661b9f3a61a032dbd6f");
        }

        private static void Case_04044()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4044,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-6,11,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-14,12,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,18,85,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,6,69,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,2,43,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-14,69,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-16,99,1,2), new GeneratedEnemyUnit(-13,3,34,19,1), new GeneratedEnemyUnit(18,15,16,37,3), new GeneratedEnemyUnit(-2,10,63,39,1), new GeneratedEnemyUnit(7,9,84,8,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "d6dc734546c59ad591a9890cf6380bee1b0467d4dfdf67e14ce74c47cd32a840");
        }

        private static void Case_04045()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4045,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,4,29,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,15,75,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,10,82,17,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "bf747d6b581ee2f5a8b07af91805905717bcfc6e7c9e8d9104e7b578d81b28b4");
        }

        private static void Case_04046()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4046,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,6,70,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,1,66,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-18,70,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,3,24,34,2), new GeneratedEnemyUnit(-11,-11,97,42,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "2ae5cd5117fbc2605e6bcbb41bd5ba392145469dbabdc6cd73d602d004823972");
        }

        private static void Case_04047()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4047,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,4,74,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,6,91,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-10,25,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-20,42,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-17,82,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,19,66,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-19,66,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,4,42,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-19,99,14,1), new GeneratedEnemyUnit(-3,-18,38,10,1), new GeneratedEnemyUnit(-11,9,93,20,4), new GeneratedEnemyUnit(-8,-14,8,20,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "b747fd47455efc904004980051f7ba1ff1f5e89b67d73351eaae5a2d49e75728");
        }

        private static void Case_04048()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4048,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,4,11,1,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "2b027b9b61eaf91dff074ce80c4e108e5c327d0a0dbf7dde9d6d6069ec199e36");
        }

        private static void Case_04049()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4049,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,10,77,5,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,4,47,36,4), new GeneratedEnemyUnit(20,-5,64,24,1), new GeneratedEnemyUnit(11,-15,51,11,4), new GeneratedEnemyUnit(-5,-14,73,37,1), new GeneratedEnemyUnit(15,-16,20,18,4), new GeneratedEnemyUnit(7,1,91,26,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "b1517b42ea9424727b50bccb8aab992cabdc0c2e6b3ba4dc4f02fa800ad490a2");
        }

        private static void Case_04050()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4050,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,13,38,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,4,74,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,2,19,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-2,16,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,13,40,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,11,18,12,3), new GeneratedEnemyUnit(3,10,96,27,3), new GeneratedEnemyUnit(-15,1,5,31,1), new GeneratedEnemyUnit(13,3,20,17,2), new GeneratedEnemyUnit(-4,3,82,33,4), new GeneratedEnemyUnit(-1,-9,27,43,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "f509dce45a0b476d536f856072831c403f692d6acf11f2ae1f000e273a52d262");
        }

        private static void Case_04051()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4051,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,20,42,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-16,95,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-1,39,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-5,34,25,4), new GeneratedEnemyUnit(-1,-4,92,17,3), new GeneratedEnemyUnit(4,6,42,32,4), new GeneratedEnemyUnit(-2,-8,40,2,4), new GeneratedEnemyUnit(-10,-8,78,36,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "c13005075fd0a1bac749d0100251595ba41a5a9776e45d6dca9d2cc1605ffc79");
        }

        private static void Case_04052()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4052,
                posture: BotStrategicPosture.Search,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,0,20,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,3,92,6,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "dd34193ca9f9950242985fcd63f69e4b10751f802594a2ea2d2d4a282794ac94");
        }

        private static void Case_04053()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4053,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,11,25,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,15,59,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,3,80,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-13,68,12,3), new GeneratedEnemyUnit(2,-7,55,39,3), new GeneratedEnemyUnit(18,8,39,33,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "66ef156129017f4918279c77e3371919c5e00487940d0fd803848450aab1b453");
        }

        private static void Case_04054()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4054,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,10,78,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,18,50,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,17,30,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,15,6,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-17,89,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-18,35,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,16,75,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,2,7,5,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "f434e3b7bb81c450493702cdf69528a672b3beacdf49787e3375e8e324d4a8a6");
        }

        private static void Case_04055()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4055,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,17,77,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,14,65,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,4,48,49,1), new GeneratedEnemyUnit(9,-10,56,26,4), new GeneratedEnemyUnit(7,-3,14,34,4), new GeneratedEnemyUnit(0,-9,63,11,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "d49ca82f29b128d30214161c22d55aa16a8a96f06e59d708cd7a1c789f334ca7");
        }

        private static void Case_04056()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4056,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-20,22,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,14,76,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,6,31,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,1,95,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-8,44,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-2,24,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-11,35,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,17,43,36,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "9e439f1abf0cc455fb88b46ffc8e94a3603616acb5c2260caeb3e5ca666c7779");
        }

        private static void Case_04057()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4057,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,10,66,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-20,84,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,18,33,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,8,40,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,19,9,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,11,86,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-16,53,2,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,11,24,40,1), new GeneratedEnemyUnit(-7,-1,40,34,2), new GeneratedEnemyUnit(20,2,60,39,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "1bcc6816ab489d4a6f32cb38e5b9e84561a4e077ff778fb2e7d56ef64c81e221");
        }

        private static void Case_04058()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4058,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-4,72,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,-3,40,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-16,43,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-8,42,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-8,55,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-8,29,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,9,74,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-2,66,10,3), new GeneratedEnemyUnit(-8,-14,26,12,2), new GeneratedEnemyUnit(-9,14,56,15,3), new GeneratedEnemyUnit(-1,13,87,33,2), new GeneratedEnemyUnit(7,-9,25,43,4), new GeneratedEnemyUnit(-12,-20,80,35,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 55,
                stableHash: "8d034ce051419430beaf5c4e51ae2043b0dd0f75d986178b3ab028d44d27cb95");
        }

        private static void Case_04059()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4059,
                posture: BotStrategicPosture.Opening,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-5,13,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,7,16,19,4), new GeneratedEnemyUnit(-1,-20,68,11,4), new GeneratedEnemyUnit(-9,-10,78,16,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "61f019f994675219751b37bf61038ddb4e27ce429dd770070bc02333ab0ed6ef");
        }

        private static void Case_04060()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4060,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,8,76,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-7,19,2,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,1,87,19,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "12e10e7a0972b3ad7ee6f68b3918916ea37ce06760f60d9f3dbcd526b38c0012");
        }

        private static void Case_04061()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4061,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-15,99,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,4,89,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-10,55,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-12,85,4,2), new GeneratedEnemyUnit(-1,-12,92,34,1), new GeneratedEnemyUnit(4,-16,39,30,2), new GeneratedEnemyUnit(-10,-14,20,47,2), new GeneratedEnemyUnit(-6,-2,68,49,3), new GeneratedEnemyUnit(-13,11,36,40,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "704e21a699ce7e2bb99977d53fb4fbf723f3d129543975c9e882cc7b5d5f420b");
        }

        private static void Case_04062()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4062,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-20,95,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-6,17,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,6,35,1,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-5,51,22,4), new GeneratedEnemyUnit(-13,-14,84,35,1), new GeneratedEnemyUnit(9,-13,22,9,4), new GeneratedEnemyUnit(-3,15,25,8,3), new GeneratedEnemyUnit(4,10,27,39,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "219dbc3d466fc29f22050eea07da7a8629d3f56b2b94642afff020703c4b2e69");
        }

        private static void Case_04063()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4063,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,0,74,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,6,93,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,18,84,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-9,84,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,20,6,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,16,57,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-10,19,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-20,24,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,7,26,36,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "7d41c4478ef8a7f0fcbf5a14207b87501dce0328e43937abb9d45e16f0c20a80");
        }

        private static void Case_04064()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4064,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-13,55,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,17,50,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,18,21,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,15,41,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-4,20,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-19,96,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,7,55,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,4,92,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,17,57,31,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "2aef0e6bcffa03c8f5bbf7828700afafbe388f60927dcfdb98eaff5a7df38b40");
        }

        private static void Case_04065()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4065,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,16,19,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-15,68,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-6,42,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-10,55,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,8,85,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-3,55,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-7,76,12,2), new GeneratedEnemyUnit(3,16,71,14,3), new GeneratedEnemyUnit(-9,4,24,6,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "52feb15ed504147eb445b99c3722a8283a1a36d4a5670cfaa0015f486b75e58a");
        }

        private static void Case_04066()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4066,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,18,80,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-15,5,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,15,51,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,-4,32,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,14,45,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,15,67,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,10,22,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,12,85,5,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-17,8,4,2), new GeneratedEnemyUnit(15,15,11,44,4), new GeneratedEnemyUnit(-11,16,27,9,3), new GeneratedEnemyUnit(-13,-18,83,4,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "b690698d4c2d8c40a2b00c63501d025f131f3f9c7a4410d39de9a20110ea2d95");
        }

        private static void Case_04067()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4067,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-5,53,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,2,62,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-20,51,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,-5,55,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-5,59,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,3,31,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,4,13,6,2,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "525b50b68bc51049ad25c6fc4d3814949815ebab8c7966305991236dd9fb70c1");
        }

        private static void Case_04068()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4068,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-14,26,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-9,62,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-16,95,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(18,-19,69,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-17,37,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,9,65,3,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,19,39,42,4), new GeneratedEnemyUnit(-17,6,38,11,1), new GeneratedEnemyUnit(-16,17,94,15,3), new GeneratedEnemyUnit(10,-9,69,38,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "681fb653d09f32a5e8e023152062bc2352b5e2efbc0ffd23aa1319fc18cce843");
        }

        private static void Case_04069()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4069,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-2,38,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,19,80,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-6,38,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,5,82,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,-13,32,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,16,57,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-6,45,50,3), new GeneratedEnemyUnit(2,-5,22,50,4), new GeneratedEnemyUnit(-5,-10,7,32,4), new GeneratedEnemyUnit(-19,-12,78,13,3), new GeneratedEnemyUnit(8,-1,20,11,3), new GeneratedEnemyUnit(-5,7,42,6,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "0e4ddd64b929a1dd765147dad4db4fb38c0490c392da8a0c715b4993d6bf82bc");
        }

        private static void Case_04070()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4070,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-14,86,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-20,13,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-4,70,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,6,93,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,8,60,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,13,12,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,2,28,28,3), new GeneratedEnemyUnit(-11,-8,43,36,4), new GeneratedEnemyUnit(-8,16,85,12,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "8eec92825e4f85b39cb1476a0c1ac913e63754422357e8ff6adeab36b9e9f4ba");
        }

        private static void Case_04071()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4071,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,10,39,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-8,9,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-13,26,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,1,66,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,2,38,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-5,43,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-4,7,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,17,53,39,3), new GeneratedEnemyUnit(-2,3,10,38,4), new GeneratedEnemyUnit(2,2,17,33,4), new GeneratedEnemyUnit(5,0,96,14,1), new GeneratedEnemyUnit(20,12,31,35,2), new GeneratedEnemyUnit(7,3,10,18,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "fdf2ea7fa6179d47a852a178e9069980779ed2abde04d6eadc6155a881fcf9d7");
        }

        private static void Case_04072()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4072,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,1,78,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,2,51,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-6,74,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,17,66,6,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,17,76,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,4,23,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,13,93,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-7,36,7,2,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "02be4610e55ef1fbe65bfb332a6471441aafa6a8e3ff1cfcc54bca131772b140");
        }

        private static void Case_04073()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4073,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,1,12,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,-1,92,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,1,33,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-7,39,25,3), new GeneratedEnemyUnit(18,-8,89,42,4), new GeneratedEnemyUnit(6,7,24,39,3), new GeneratedEnemyUnit(-1,-1,89,31,2), new GeneratedEnemyUnit(13,19,56,30,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "8dd4f3475776084247d48cc6263b0bf9cc7cdaca714b4afb42271f44a1466667");
        }

        private static void Case_04074()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4074,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,4,5,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-12,81,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-18,90,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,13,84,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-10,50,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-4,19,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-20,46,43,3), new GeneratedEnemyUnit(-10,8,66,19,1), new GeneratedEnemyUnit(-13,-1,25,11,2), new GeneratedEnemyUnit(-1,-20,76,8,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "bcb402345ad443a92102894c30ce9341ed51bb5068a48f106a61b631245036ee");
        }

        private static void Case_04075()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4075,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,9,85,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-1,90,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-15,14,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,16,22,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-11,-20,82,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-1,16,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-11,56,19,4), new GeneratedEnemyUnit(-2,15,71,37,1), new GeneratedEnemyUnit(11,-20,53,41,3), new GeneratedEnemyUnit(2,-10,45,19,3), new GeneratedEnemyUnit(2,14,15,6,2), new GeneratedEnemyUnit(15,16,65,14,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "9740853e3f87f8b56fcf223015f7c16b83cec2513cfe80e767db63dbe6572859");
        }

        private static void Case_04076()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4076,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-13,73,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,-6,62,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,8,73,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,4,75,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-11,38,1,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,6,6,29,3), new GeneratedEnemyUnit(14,-19,80,13,1), new GeneratedEnemyUnit(-7,4,56,18,2), new GeneratedEnemyUnit(-14,14,94,8,2), new GeneratedEnemyUnit(19,12,63,25,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "9d3a3084dfcb59f3dbfc5640f4bd70eb6975b4d81217b5f7cea6f5c78e7af468");
        }

        private static void Case_04077()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4077,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,2,86,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,-17,29,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-12,-14,47,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,3,18,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-1,49,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,11,61,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-3,55,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,0,58,30,4), new GeneratedEnemyUnit(-13,17,74,47,4), new GeneratedEnemyUnit(-3,10,68,50,1), new GeneratedEnemyUnit(-11,18,50,6,3), new GeneratedEnemyUnit(19,-5,100,45,2), new GeneratedEnemyUnit(19,0,98,3,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "795b32d75d45da7c5c273414b69a26c207f14fa9236a7a3fc65c5ee1c34d00a8");
        }

        private static void Case_04078()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4078,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,-4,63,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,-20,11,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,8,65,37,3), new GeneratedEnemyUnit(-7,5,21,49,2), new GeneratedEnemyUnit(7,-18,97,34,1), new GeneratedEnemyUnit(-3,-19,39,24,1), new GeneratedEnemyUnit(-16,17,14,10,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 55,
                stableHash: "2a2b970ee8069182bd7368e321ef3171cbb396ced76d483e333c9b306212a2f4");
        }

        private static void Case_04079()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4079,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-5,93,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,1,37,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,16,68,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-8,83,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-2,87,11,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "9cded804942833d492d1f8451c3717a4781efda6423322d5c93b850a9006cb75");
        }

        private static void Case_04080()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4080,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-19,89,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,14,48,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,-1,54,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-12,59,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-11,20,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,8,31,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-13,78,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,-16,86,2,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-2,79,26,4), new GeneratedEnemyUnit(13,-17,36,25,3), new GeneratedEnemyUnit(-8,3,66,33,3), new GeneratedEnemyUnit(-10,8,76,11,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "ed88df2ec628cc05a335cc8c883aa61ab875418081db21940fd8403263ceb4c1");
        }

        private static void Case_04081()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4081,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,11,34,4,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "cd8dcc3c24b4e5345b635990768655994d505c16efa16d4b1cbdc0902c768f7e");
        }

        private static void Case_04082()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4082,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-14,42,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,19,44,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,17,81,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,10,17,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-10,69,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-14,5,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,16,37,3,3), new GeneratedEnemyUnit(-11,-18,8,34,4), new GeneratedEnemyUnit(19,16,62,42,3), new GeneratedEnemyUnit(1,12,48,10,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "264e6b9bca64c7e380730113a26db7afba078954a2517a725596c3314c44fe3b");
        }

        private static void Case_04083()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4083,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,19,63,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,7,95,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-14,27,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-16,95,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-11,70,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,4,17,33,2), new GeneratedEnemyUnit(-14,20,25,29,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "ab77cc3777ecda2de56181d0d1b730cce95a230533d648417d7e327f79e5b264");
        }

        private static void Case_04084()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4084,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,20,27,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,-15,5,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,4,33,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-16,5,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,18,37,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,-3,19,3,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,20,41,4,3), new GeneratedEnemyUnit(-6,6,96,13,4), new GeneratedEnemyUnit(-16,9,34,12,2), new GeneratedEnemyUnit(-9,15,32,8,1), new GeneratedEnemyUnit(5,-3,78,35,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "30e893629152df02471345fb67868757fbf314130e7c8f4e7b1547abd7076350");
        }

        private static void Case_04085()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4085,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-6,54,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,9,35,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-3,84,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-15,67,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,20,98,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-17,83,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-13,90,34,1), new GeneratedEnemyUnit(-20,7,77,19,4), new GeneratedEnemyUnit(0,-9,33,15,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "1ebab91ab3ee582749e1fad2e8aed6c3d8815f875dd56d019f5297346d7c3dcc");
        }

        private static void Case_04086()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4086,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,1,52,4,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "d496adb9fd5e769cc41f965a6b0b1c323bb685339dcfb0f69339ffb5285e8b06");
        }

        private static void Case_04087()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4087,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,14,65,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-10,57,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-3,72,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-9,-15,44,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,-1,88,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,18,83,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-7,78,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-4,52,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,8,59,10,2), new GeneratedEnemyUnit(14,-17,5,10,1), new GeneratedEnemyUnit(18,14,92,3,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "3ea0ee5a0687d28eed1af2dced22fff2b2396e601fbf5ce75d9fdb174d0bab82");
        }

        private static void Case_04088()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4088,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,7,67,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,8,81,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,17,76,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,0,9,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,11,17,23,2), new GeneratedEnemyUnit(9,7,26,42,4), new GeneratedEnemyUnit(0,-14,53,16,1), new GeneratedEnemyUnit(-6,2,54,47,2), new GeneratedEnemyUnit(-12,1,61,17,2), new GeneratedEnemyUnit(-8,-2,21,24,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "f7e7cb73c40d89b5db6f7bf806f06b7528b3825074b14b9df370d2cdbf8268d0");
        }

        private static void Case_04089()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4089,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,9,63,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-7,30,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,11,91,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,7,68,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-12,90,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-4,62,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,17,9,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,18,27,42,3), new GeneratedEnemyUnit(-19,-14,48,9,4), new GeneratedEnemyUnit(-12,19,55,32,3), new GeneratedEnemyUnit(3,-12,98,47,2), new GeneratedEnemyUnit(19,-5,81,14,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "6c3c7cd392a5a2b0432611fee07b6f80a949b44e0fa050bcdbd028243e995d4a");
        }

        private static void Case_04090()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4090,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-4,47,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,18,68,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,12,29,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,17,73,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,11,95,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-4,62,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,-9,10,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,2,90,2,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,0,8,19,2), new GeneratedEnemyUnit(4,-10,83,30,4), new GeneratedEnemyUnit(-12,12,29,12,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "7b10854beeadf34f4acee6cc91cc7f2bd445450d8ec4bed4e541f404234223f0");
        }

        private static void Case_04091()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4091,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,4,34,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-14,99,3,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-3,86,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,20,74,15,1), new GeneratedEnemyUnit(-2,5,70,35,4), new GeneratedEnemyUnit(1,4,20,38,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "98c06d4c6f445a438a0fcc9929486c3469058f617cce9b0f06d23e426b928eb3");
        }

        private static void Case_04092()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4092,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,17,98,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-4,22,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-12,98,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-6,10,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(15,1,45,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-17,72,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-14,18,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-12,49,18,1), new GeneratedEnemyUnit(15,-2,37,3,1), new GeneratedEnemyUnit(-20,6,54,13,4), new GeneratedEnemyUnit(-19,-11,98,45,4), new GeneratedEnemyUnit(-1,19,77,39,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "e32bebc63e2717c491aff030a8210f994486be8489463f6b17d8e8f390593af0");
        }

        private static void Case_04093()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4093,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,14,17,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,16,52,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,10,59,20,2), new GeneratedEnemyUnit(-20,-20,40,16,4), new GeneratedEnemyUnit(11,19,6,33,2), new GeneratedEnemyUnit(8,-10,11,38,1), new GeneratedEnemyUnit(3,5,87,3,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "dae1687cb3cc7ed48b73afcb8dee2b5d2385ef82c78abd0655977eb241fbf5c2");
        }

        private static void Case_04094()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4094,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,11,47,1,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "f6e64f65d1812b01baa989c4bcacd5bd79b66382b66dfaf746913d46d6ceea97");
        }

        private static void Case_04095()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4095,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,0,84,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,8,23,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,16,56,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,-9,22,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-3,14,6,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,18,11,27,3), new GeneratedEnemyUnit(19,13,49,8,2), new GeneratedEnemyUnit(9,-6,51,49,4), new GeneratedEnemyUnit(-15,13,25,33,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "51fb8450dad102435a24730dd560e604da3a96c48c37e66ec5485ca29da179db");
        }

        private static void Case_04096()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4096,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-2,16,2,2,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "428060e8e4c42e4941bd71cecaae8ac25950685ec2ca172d81ee6800d21344e2");
        }

        private static void Case_04097()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4097,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,0,60,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,18,41,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,9,27,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,16,70,2,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-4,30,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-7,39,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,8,80,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-18,38,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,-8,53,1,3), new GeneratedEnemyUnit(8,-11,61,35,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "86ffe28ac76136e63d9a752335279dd3659efc6c879eb8db17cb8313e27b3ecb");
        }

        private static void Case_04098()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4098,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,16,72,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,16,82,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,0,60,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,16,18,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-4,44,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-3,11,7,1,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "9e4de9d42496627abd68a5590ee752dd582a5dae5905a838009834150af0bcdb");
        }

        private static void Case_04099()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4099,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-13,19,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,12,33,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-12,46,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-6,10,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(1,-20,72,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-3,41,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-13,6,47,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,4,97,7,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "493f6708952ef99f48ecd4d7b960f00e0f3876c7be82d82309296cbd20d45751");
        }

        private static void Case_04100()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4100,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-1,98,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,18,72,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-16,60,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-18,99,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,8,18,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-11,27,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-6,100,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,3,72,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-3,93,23,3), new GeneratedEnemyUnit(-1,-13,37,8,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "00a2de14d207c76519dfaad98ff258c6d6f7400ad4a2d02337480c68c95c4497");
        }

        private static void Case_04101()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4101,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-13,12,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,16,53,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-15,29,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-18,100,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-8,16,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-4,73,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,-7,100,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-8,13,1,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,20,8,16,1), new GeneratedEnemyUnit(17,-10,90,32,1), new GeneratedEnemyUnit(5,14,15,19,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "7734ab5cd0f1aad5413eee27900749255e7353d8725b6e9e733c20dbe28c51bc");
        }

        private static void Case_04102()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4102,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-20,57,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,12,88,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,14,52,42,2), new GeneratedEnemyUnit(19,12,71,41,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "02eb5346c37630258484153d2088db5170bde8ab73920c13813ed7710b38446c");
        }

        private static void Case_04103()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4103,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-14,15,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,9,12,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,16,55,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-18,52,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-16,84,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-6,13,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,7,43,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-3,15,18,1), new GeneratedEnemyUnit(2,3,43,18,3), new GeneratedEnemyUnit(-12,-9,89,1,3), new GeneratedEnemyUnit(-19,1,38,32,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "d7c50d3f0ed2e2ea4de2ca8215b71d19f7d2d50f852257c844a819ea5d4bf699");
        }

        private static void Case_04104()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4104,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,15,97,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-11,50,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-10,29,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-18,40,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,19,41,38,3), new GeneratedEnemyUnit(13,7,86,9,1), new GeneratedEnemyUnit(-14,-8,91,22,4), new GeneratedEnemyUnit(10,-2,63,12,2), new GeneratedEnemyUnit(-14,14,24,31,4), new GeneratedEnemyUnit(-20,-14,41,48,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "365e1657dbabebe650d7d0be0669644898467c594bdeef13b3f8426a716463dd");
        }

        private static void Case_04105()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4105,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,0,89,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-8,45,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,13,63,2,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-10,11,34,2), new GeneratedEnemyUnit(-2,5,48,5,3), new GeneratedEnemyUnit(4,-17,14,30,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "b3a41da6ec95afaae60beea931f69ab6f78ba0b43134206ccd9da17d046f86e0");
        }

        private static void Case_04106()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4106,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,18,100,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,2,70,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-5,85,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,-5,7,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-4,60,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,5,10,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-13,18,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-6,20,2,3,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "95d5e973494ebc826f6b2bf5798d44b1ce8398f882d237004a348c22e986bdd6");
        }

        private static void Case_04107()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4107,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-16,84,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,-3,86,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,18,67,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,-2,9,6,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-4,35,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-17,82,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-15,48,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,5,22,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,-14,81,31,2), new GeneratedEnemyUnit(0,3,24,29,2), new GeneratedEnemyUnit(-6,7,42,20,4), new GeneratedEnemyUnit(-1,17,66,28,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "6eaf115027352bfc11aa9e8701be47ce6996491c842b943f41fed4fae165b8b8");
        }

        private static void Case_04108()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4108,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,7,26,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-18,27,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,6,94,19,2), new GeneratedEnemyUnit(5,20,32,30,3), new GeneratedEnemyUnit(5,4,67,25,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 85,
                stableHash: "7dcc9cb4c65fd8f0d2b6368855037057196e9134158ddf75b917fe1dd7b2dd81");
        }

        private static void Case_04109()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4109,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-15,62,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-4,99,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,-13,21,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,4,98,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,19,58,23,4), new GeneratedEnemyUnit(-3,9,34,40,1), new GeneratedEnemyUnit(-19,-9,45,31,1), new GeneratedEnemyUnit(5,-18,92,13,4), new GeneratedEnemyUnit(5,-5,45,27,4), new GeneratedEnemyUnit(-19,-14,7,31,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "c9e9abe69491ef532a56d067f13aa0cd2961e5f5a505896dff38fa70f3e6079b");
        }

        private static void Case_04110()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4110,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-18,98,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,19,19,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,13,47,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,1,64,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-1,35,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-15,41,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,4,38,4,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-4,73,34,2), new GeneratedEnemyUnit(16,-13,100,14,1), new GeneratedEnemyUnit(-8,2,19,50,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "e1db9374c05f2ae911aacfa8f3e34f3fd063e6686c16e3a00bd1337fe5f7819b");
        }

        private static void Case_04111()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4111,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,6,68,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,10,59,7,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,5,64,10,3), new GeneratedEnemyUnit(15,-18,97,19,1), new GeneratedEnemyUnit(16,4,62,34,4), new GeneratedEnemyUnit(1,-16,91,48,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "850339988e8e3ee641e13792a92dba2901472cc0ea8f5698caadf758f76180c9");
        }

        private static void Case_04112()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4112,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,9,22,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,13,5,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,10,65,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,15,23,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,20,48,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,15,92,7,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 70,
                stableHash: "290b18aa75dcabf80c1d2535aa2b31568b1eaa4cadcb214cb7fd660fda526c5e");
        }

        private static void Case_04113()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4113,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,17,42,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,4,59,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,14,40,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-6,39,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-13,45,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,17,48,6,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-20,35,48,4), new GeneratedEnemyUnit(-13,9,49,27,4), new GeneratedEnemyUnit(-15,-18,41,1,3), new GeneratedEnemyUnit(15,-7,94,42,1), new GeneratedEnemyUnit(-8,17,28,17,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "9789aa8d115d28b06f83700f624e3ce3bc356c8cb7c915474fa127d4cb0a2e1b");
        }

        private static void Case_04114()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4114,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,13,23,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,14,98,38,3), new GeneratedEnemyUnit(8,-19,68,7,3), new GeneratedEnemyUnit(15,-9,78,34,2), new GeneratedEnemyUnit(4,5,56,21,1), new GeneratedEnemyUnit(-20,-4,49,22,4), new GeneratedEnemyUnit(2,9,100,46,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "4a2a06a83adbeaf2e8858a8e0e1bd19c26c82d6b63347e7a82d73cebef264779");
        }

        private static void Case_04115()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4115,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-14,57,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,6,55,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,20,31,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,15,80,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-5,72,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,16,88,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-14,44,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-3,35,14,2), new GeneratedEnemyUnit(11,5,13,41,3), new GeneratedEnemyUnit(8,-12,44,11,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "dbd69295854b03a69c2781e69f72ce9e7fe90ee849ce60ef6cf25b9bd844166c");
        }

        private static void Case_04116()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4116,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,-17,30,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,1,9,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-12,9,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,10,97,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,3,6,3,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "ee55e4be63263c24f5020ed3c008e3728fc48297438d93651f98956991515aef");
        }

        private static void Case_04117()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4117,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,16,15,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,4,8,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,6,19,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,16,100,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,6,19,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-10,69,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,11,36,6,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,1,78,9,1), new GeneratedEnemyUnit(-11,17,36,30,4), new GeneratedEnemyUnit(20,-2,40,48,4), new GeneratedEnemyUnit(4,0,61,33,2), new GeneratedEnemyUnit(-13,12,52,19,3), new GeneratedEnemyUnit(-17,-12,30,4,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "a21073349d258deeeb607053741bb58854a388c4dba79441bbdc56c3414b20f5");
        }

        private static void Case_04118()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4118,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,8,70,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,10,41,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-19,21,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-12,-10,12,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,19,95,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,-18,98,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-5,23,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-20,54,42,1), new GeneratedEnemyUnit(17,-17,23,2,4), new GeneratedEnemyUnit(-3,-16,40,21,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "5bf40978db0d389206f4e20c72ff7b4ed8556097a1b988114ea1e357e54b97ba");
        }

        private static void Case_04119()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4119,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,11,46,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-13,-17,94,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,9,37,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,8,27,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-5,52,48,2), new GeneratedEnemyUnit(7,-2,11,14,2), new GeneratedEnemyUnit(-12,-19,19,12,3), new GeneratedEnemyUnit(-17,-2,92,28,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "e66967c1213b36cf6ad0a538d80cdd5b2208096763eac80f1df3170a629587ad");
        }

        private static void Case_04120()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4120,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-6,70,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-14,34,7,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,-19,50,46,1), new GeneratedEnemyUnit(-10,-12,87,49,2), new GeneratedEnemyUnit(1,0,33,17,4), new GeneratedEnemyUnit(20,15,78,41,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "4229822cf841288f6173987b4200834919cfa04da773102d12767b698f7de18e");
        }

        private static void Case_04121()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4121,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,-16,49,6,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-6,28,11,1), new GeneratedEnemyUnit(17,-5,59,13,2), new GeneratedEnemyUnit(10,3,40,18,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "10a704040c2fa0661bee93d9e3447062682edb97a680e7d45c156146d32f00b9");
        }

        private static void Case_04122()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4122,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,20,98,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,13,93,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,1,84,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,0,30,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-3,64,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-19,97,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(10,0,88,34,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "3aec3ab5e77474612eb7a6a7764d07d3dbd9ec777f1168657cf83f62489cd1a4");
        }

        private static void Case_04123()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4123,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-19,93,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-7,62,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,11,87,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,19,44,14,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "1a3fefd15e2626eb935d17a5f5051bfd9bb664e06aea067e2be6842a9be0c937");
        }

        private static void Case_04124()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4124,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,16,25,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,13,74,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,13,10,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,-6,27,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-11,54,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-20,78,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,18,25,4,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "3d293cb2847cd2b4f985c62758d4df6d0e8d5554abd496113d6ede868c63a323");
        }

        private static void Case_04125()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4125,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-20,13,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-7,28,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,13,89,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-1,10,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,-10,27,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-19,33,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,12,31,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,18,15,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,18,61,42,4), new GeneratedEnemyUnit(-8,-1,33,7,4), new GeneratedEnemyUnit(-9,1,83,42,2), new GeneratedEnemyUnit(-13,1,73,15,4), new GeneratedEnemyUnit(-4,5,11,16,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "28503aa70dec4503e226827a76fbb6082eee19e985664eaac0c36923a2ceb89e");
        }

        private static void Case_04126()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4126,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,3,98,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,0,13,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,5,17,6,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-7,91,23,1), new GeneratedEnemyUnit(0,-4,74,30,1), new GeneratedEnemyUnit(12,2,7,23,1), new GeneratedEnemyUnit(11,1,68,8,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "101a4ef458b3febf8fbfbc36b7bfaf4c03c9532f6fb52ee681a67e74d25d8c33");
        }

        private static void Case_04127()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4127,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-9,19,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,16,13,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,20,70,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-14,61,1,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-14,84,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-19,11,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,13,68,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,16,7,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-17,15,1,2), new GeneratedEnemyUnit(3,9,35,7,1), new GeneratedEnemyUnit(-1,-13,23,27,3), new GeneratedEnemyUnit(4,-10,50,25,2), new GeneratedEnemyUnit(4,0,93,35,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "366deda28885f7ff67e367117b2af71b895773719d47ea0e98798c87f91afd58");
        }

        private static void Case_04128()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4128,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-1,24,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-11,7,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-9,76,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,1,22,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-12,27,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,18,70,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,17,66,7,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-19,80,42,1), new GeneratedEnemyUnit(14,-5,88,33,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "5989b8d02cbfb369bd493a98f101a11086bba1195ee13ee64bf354fd1076b708");
        }

        private static void Case_04129()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4129,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,13,34,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,20,44,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,3,16,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,11,43,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-9,24,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-17,91,7,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,6,98,29,2), new GeneratedEnemyUnit(0,-17,50,48,3), new GeneratedEnemyUnit(20,-14,24,31,3), new GeneratedEnemyUnit(4,6,49,48,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "d5cb42cc38a032952176a3764340e2d1f2c66970a97b850fc4ea077723a9b0ef");
        }

        private static void Case_04130()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4130,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-15,93,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,-16,84,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-19,10,4,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "5e1cf016a10da0a85ffd9fde700df68f5ce0dbfdf88d2094469ab8dfa44ba91a");
        }

        private static void Case_04131()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4131,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,2,71,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-8,20,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,7,62,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-8,60,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,1,69,3,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "3b0d99425db653c77512f67cdfdd8ae6807928ceedaf57047a52dce202452c73");
        }

        private static void Case_04132()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4132,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-11,33,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-17,12,3,3), new GeneratedEnemyUnit(-14,8,48,35,4), new GeneratedEnemyUnit(17,-9,14,37,4), new GeneratedEnemyUnit(-16,2,90,6,3), new GeneratedEnemyUnit(5,8,94,16,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "83fde307f6be76e931b144f03a95f7ef3b58aa4accbad54330ddd5fb4e079d37");
        }

        private static void Case_04133()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4133,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,11,33,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,17,80,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,16,6,37,1), new GeneratedEnemyUnit(17,-15,97,9,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "aed8d251418abd88b9eb87c8efed33f63f6fb678455a68dd6b336a621140d08c");
        }

        private static void Case_04134()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4134,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,15,48,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,9,19,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,7,91,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(0,8,78,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-10,85,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,-11,92,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,5,45,5,4), new GeneratedEnemyUnit(6,15,59,45,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "41d85967e3fed7594538e98993a02cabda7f12f9b8728af16650d5c123870d18");
        }

        private static void Case_04135()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4135,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-4,14,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-19,64,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-12,51,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-3,49,5,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,12,88,34,3), new GeneratedEnemyUnit(-19,-13,68,29,4), new GeneratedEnemyUnit(2,-10,15,2,4), new GeneratedEnemyUnit(15,16,12,24,4), new GeneratedEnemyUnit(-20,17,55,29,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "c4447841edbdb5630b33832643820b4866844ff5b5c56c1e749f0c506b388fb1");
        }

        private static void Case_04136()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4136,
                posture: BotStrategicPosture.Opening,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-7,17,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,20,37,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,7,92,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-17,55,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-15,74,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,11,59,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-15,60,22,4), new GeneratedEnemyUnit(4,-7,93,47,3), new GeneratedEnemyUnit(10,12,67,17,3), new GeneratedEnemyUnit(9,-18,95,28,3), new GeneratedEnemyUnit(-18,6,20,20,2), new GeneratedEnemyUnit(14,19,60,35,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "9858bebbc16f36bb1e9535d4f69029a21c6bd09144dc3fcc8b31712f53f4a542");
        }

        private static void Case_04137()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4137,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-7,65,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-16,95,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-8,69,26,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "70113511078fc0aa04d2c881abe5c4d6fc385ece33de30e852cd6121329b6a11");
        }

        private static void Case_04138()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4138,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,15,95,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,10,28,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,14,74,33,2), new GeneratedEnemyUnit(-11,-10,82,50,3), new GeneratedEnemyUnit(15,2,35,35,1), new GeneratedEnemyUnit(-2,-15,72,35,1), new GeneratedEnemyUnit(18,17,65,44,1), new GeneratedEnemyUnit(6,-5,20,31,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "86576acffebebcad4de183818a919d608dccde4363632332763386427a69174a");
        }

        private static void Case_04139()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 4139,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,5,71,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-11,56,25,4), new GeneratedEnemyUnit(3,-19,67,37,1), new GeneratedEnemyUnit(-13,-14,15,28,3), new GeneratedEnemyUnit(-15,-15,60,15,4), new GeneratedEnemyUnit(-10,-5,67,45,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 100,
                stableHash: "9a8f5a199bbd0b11a763c47102505a62607592fea3649640da370d3f5d80af93");
        }

    }
}
