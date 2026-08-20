using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    [TestFixture]
    public sealed class BotGeneratedTacticalRegressionShard001
    {
        [Test]
        public void RunCompiledRegressionShard()
        {
            Case_00180();
            Case_00181();
            Case_00182();
            Case_00183();
            Case_00184();
            Case_00185();
            Case_00186();
            Case_00187();
            Case_00188();
            Case_00189();
            Case_00190();
            Case_00191();
            Case_00192();
            Case_00193();
            Case_00194();
            Case_00195();
            Case_00196();
            Case_00197();
            Case_00198();
            Case_00199();
            Case_00200();
            Case_00201();
            Case_00202();
            Case_00203();
            Case_00204();
            Case_00205();
            Case_00206();
            Case_00207();
            Case_00208();
            Case_00209();
            Case_00210();
            Case_00211();
            Case_00212();
            Case_00213();
            Case_00214();
            Case_00215();
            Case_00216();
            Case_00217();
            Case_00218();
            Case_00219();
            Case_00220();
            Case_00221();
            Case_00222();
            Case_00223();
            Case_00224();
            Case_00225();
            Case_00226();
            Case_00227();
            Case_00228();
            Case_00229();
            Case_00230();
            Case_00231();
            Case_00232();
            Case_00233();
            Case_00234();
            Case_00235();
            Case_00236();
            Case_00237();
            Case_00238();
            Case_00239();
            Case_00240();
            Case_00241();
            Case_00242();
            Case_00243();
            Case_00244();
            Case_00245();
            Case_00246();
            Case_00247();
            Case_00248();
            Case_00249();
            Case_00250();
            Case_00251();
            Case_00252();
            Case_00253();
            Case_00254();
            Case_00255();
            Case_00256();
            Case_00257();
            Case_00258();
            Case_00259();
            Case_00260();
            Case_00261();
            Case_00262();
            Case_00263();
            Case_00264();
            Case_00265();
            Case_00266();
            Case_00267();
            Case_00268();
            Case_00269();
            Case_00270();
            Case_00271();
            Case_00272();
            Case_00273();
            Case_00274();
            Case_00275();
            Case_00276();
            Case_00277();
            Case_00278();
            Case_00279();
            Case_00280();
            Case_00281();
            Case_00282();
            Case_00283();
            Case_00284();
            Case_00285();
            Case_00286();
            Case_00287();
            Case_00288();
            Case_00289();
            Case_00290();
            Case_00291();
            Case_00292();
            Case_00293();
            Case_00294();
            Case_00295();
            Case_00296();
            Case_00297();
            Case_00298();
            Case_00299();
            Case_00300();
            Case_00301();
            Case_00302();
            Case_00303();
            Case_00304();
            Case_00305();
            Case_00306();
            Case_00307();
            Case_00308();
            Case_00309();
            Case_00310();
            Case_00311();
            Case_00312();
            Case_00313();
            Case_00314();
            Case_00315();
            Case_00316();
            Case_00317();
            Case_00318();
            Case_00319();
            Case_00320();
            Case_00321();
            Case_00322();
            Case_00323();
            Case_00324();
            Case_00325();
            Case_00326();
            Case_00327();
            Case_00328();
            Case_00329();
            Case_00330();
            Case_00331();
            Case_00332();
            Case_00333();
            Case_00334();
            Case_00335();
            Case_00336();
            Case_00337();
            Case_00338();
            Case_00339();
            Case_00340();
            Case_00341();
            Case_00342();
            Case_00343();
            Case_00344();
            Case_00345();
            Case_00346();
            Case_00347();
            Case_00348();
            Case_00349();
            Case_00350();
            Case_00351();
            Case_00352();
            Case_00353();
            Case_00354();
            Case_00355();
            Case_00356();
            Case_00357();
            Case_00358();
            Case_00359();
        }

        private static void Case_00180()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 180,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,20,17,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-11,68,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-20,77,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,8,100,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,16,92,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,-11,83,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,15,48,50,3), new GeneratedEnemyUnit(12,-6,25,32,4), new GeneratedEnemyUnit(-14,-1,79,33,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "2b6e4984648f4323beab9e1be7faa721ccfaf557187be614012d348521fd75e8");
        }

        private static void Case_00181()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 181,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-9,51,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,12,85,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,-12,33,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,-13,79,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,19,40,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,10,42,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-15,23,7,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-1,25,29,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "311c991765569c3a45171b5c436965024d45768e715cb8b5191d3bf03ac8bb2f");
        }

        private static void Case_00182()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 182,
                posture: BotStrategicPosture.Search,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,10,13,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,-2,52,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,-17,22,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-15,25,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-8,76,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,14,83,11,3), new GeneratedEnemyUnit(15,17,79,10,2), new GeneratedEnemyUnit(0,-1,99,34,1), new GeneratedEnemyUnit(0,-11,55,37,3), new GeneratedEnemyUnit(6,19,7,7,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 55,
                stableHash: "4ab5914cd57f641c11aaff4b9fe0f4d913cabd1d399e8e6115562c7e2cf610ff");
        }

        private static void Case_00183()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 183,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,5,20,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,-20,100,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,10,59,5,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-13,75,30,2), new GeneratedEnemyUnit(13,0,85,11,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "964a11a016ec63889d3089c28a90d2f782bee11900bc9e1edb5229b6c5a686bf");
        }

        private static void Case_00184()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 184,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-20,98,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,12,5,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,-20,69,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,19,24,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-16,45,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-3,54,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,13,64,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,10,98,5,3), new GeneratedEnemyUnit(17,-20,81,47,1), new GeneratedEnemyUnit(8,0,89,14,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "d33bdb10f0661cdd66f653ee4a62802333ceff4dfd0b2e42c3cb35569c57b1e7");
        }

        private static void Case_00185()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 185,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,5,38,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-13,45,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,6,71,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,13,85,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-18,99,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-3,47,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-17,24,48,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "685719bf875c58d4637e2f84b9e1ec39023e59c78a54b1358d726054106c5fe2");
        }

        private static void Case_00186()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 186,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-13,-16,30,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(17,-8,43,4,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-8,86,39,3), new GeneratedEnemyUnit(2,19,65,3,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "123b7579fd8d230b8ae9d0759fb660f00c9839871365e0ea19255c0f310cb0b0");
        }

        private static void Case_00187()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 187,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,4,15,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,20,72,3,3,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 15,
                stableHash: "3194b8f78bc963e6bec2c925d2485bf633ee12ce21f576fb0968cb48937b0b81");
        }

        private static void Case_00188()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 188,
                posture: BotStrategicPosture.Opening,
                castleX: 0, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-10,75,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,10,100,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-2,74,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-6,61,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,8,83,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,4,95,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-4,62,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,12,38,12,3), new GeneratedEnemyUnit(12,9,23,33,4), new GeneratedEnemyUnit(18,-7,52,8,2), new GeneratedEnemyUnit(-15,13,11,3,4), new GeneratedEnemyUnit(-11,9,45,48,1), new GeneratedEnemyUnit(-16,0,67,33,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "bcea08c219b4ec52fd37fdd915d3f4e8e648a8a146781251991fafa00bc59d59");
        }

        private static void Case_00189()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 189,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,18,16,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,9,20,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-2,9,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(4,-11,27,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,2,9,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-8,98,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-2,41,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-16,8,4,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "7da15d93c13adc38fa80a6503d76b3aac3897698d3767cc77e3e24bc779512ff");
        }

        private static void Case_00190()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 190,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-1,74,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,5,63,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,2,59,32,4), new GeneratedEnemyUnit(-5,4,8,47,2), new GeneratedEnemyUnit(17,-16,94,21,1), new GeneratedEnemyUnit(-16,18,21,1,3), new GeneratedEnemyUnit(4,-11,79,18,4), new GeneratedEnemyUnit(-14,-8,67,27,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "dd8efea38a8a6ab57c58c8d5600d8d2a708132e3ffc0e066e57df45f32b4055b");
        }

        private static void Case_00191()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 191,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,11,44,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,13,67,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(20,3,50,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-14,91,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,20,45,3,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-10,9,48,1), new GeneratedEnemyUnit(8,-20,56,22,4), new GeneratedEnemyUnit(14,0,82,25,1), new GeneratedEnemyUnit(-9,12,71,44,1), new GeneratedEnemyUnit(-13,-19,69,48,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "4d725801bef35d955944d1da22ae9726753715209855c1d1d49391f19e6c7dee");
        }

        private static void Case_00192()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 192,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,14,34,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-19,6,70,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,15,42,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,13,61,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,13,66,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,5,9,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,-11,92,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,9,87,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-12,14,6,4), new GeneratedEnemyUnit(13,4,92,21,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 85,
                stableHash: "7bc1c1c41009e8ea5fe2fb03e5dffef349a4b83afd6f4d1c38dd63f639585d8f");
        }

        private static void Case_00193()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 193,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,11,53,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-11,12,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,-14,51,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,15,83,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,-14,8,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,0,78,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,19,14,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-5,69,5,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-3,97,20,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "9b89c072b5f8790b7e5c550cd0bc1e7051fba11fb4840a105e59ff1a806d22f1");
        }

        private static void Case_00194()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 194,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-19,9,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,8,27,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,-1,64,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-16,89,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,13,7,42,2), new GeneratedEnemyUnit(8,-7,71,20,1), new GeneratedEnemyUnit(19,-18,60,11,3), new GeneratedEnemyUnit(3,10,77,21,1), new GeneratedEnemyUnit(-11,-7,38,3,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "a8cafa3c3b29dab0fc275f3e1d64dd91c9f7709a00a5873a0d3767bdac3fa706");
        }

        private static void Case_00195()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 195,
                posture: BotStrategicPosture.Search,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,-4,79,4,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,3,66,47,1), new GeneratedEnemyUnit(-9,20,54,28,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "08f2835429ae7c112e2f86d192a5eb735846dd9e9596550e73d88ee14bcbb716");
        }

        private static void Case_00196()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 196,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,18,40,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,-4,62,1,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "77639edf2c10e1ffee6932c232004e8616bc230332ccc0f97484a43c10220ba8");
        }

        private static void Case_00197()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 197,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,19,82,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,3,57,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,10,92,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,19,65,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,18,21,38,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "de5fc0e67d070b93ac1e5cde6ae0e167ed3bf97ed2a9e58a4fbac1db5c4a96ce");
        }

        private static void Case_00198()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 198,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-20,35,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-20,90,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,9,34,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-14,83,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,15,45,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,15,40,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-6,25,2,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,-5,58,47,3), new GeneratedEnemyUnit(2,-12,74,27,2), new GeneratedEnemyUnit(12,-13,56,3,4), new GeneratedEnemyUnit(1,-16,64,46,3), new GeneratedEnemyUnit(16,-4,65,25,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "ca829ff78b1cd6b20ab434459402d3f2fc6cc79b06b5ecb50746c61510efa313");
        }

        private static void Case_00199()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 199,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,-7,35,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-18,-13,54,4,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-17,73,7,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "ced708f36e15b78318245774b0809e4b78d30df3d032673521c0d198f662b445");
        }

        private static void Case_00200()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 200,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-17,21,2,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,6,77,28,2), new GeneratedEnemyUnit(6,-11,69,24,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "69ad23d23d4606c7b5836785ea99171c82bfcf18be5837f86d90a5269a3f6d3c");
        }

        private static void Case_00201()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 201,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,2,97,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,1,51,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-4,45,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-20,5,16,2), new GeneratedEnemyUnit(-11,9,31,31,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "946a6b5f679bf6e488c25f517efa0adb239f8cddc26e27087b1a2c8f57cd6400");
        }

        private static void Case_00202()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 202,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,11,20,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,-10,67,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,1,23,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-4,98,3,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-17,16,36,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "3bc12e234ea8e862819699c430e210bf888b9b7d7736aba41e90f59701235922");
        }

        private static void Case_00203()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 203,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,-4,28,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-1,61,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-13,58,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-15,17,18,6,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-17,13,18,3), new GeneratedEnemyUnit(15,18,36,31,2), new GeneratedEnemyUnit(19,-6,54,34,1), new GeneratedEnemyUnit(-3,11,57,22,2), new GeneratedEnemyUnit(16,18,15,22,2), new GeneratedEnemyUnit(-2,19,69,23,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "3d4d436ed82e1000805df063b8e77739b5e8ebf2d877ec41c0198ccb51d7fa9c");
        }

        private static void Case_00204()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 204,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,13,28,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-17,72,7,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,2,9,13,3), new GeneratedEnemyUnit(11,-12,35,50,4), new GeneratedEnemyUnit(15,17,75,23,1), new GeneratedEnemyUnit(6,9,42,29,1), new GeneratedEnemyUnit(-2,-15,97,42,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "3821f339309d320002a4769d786755b50e45d1ac23a6aa9c9c0457f53787f07e");
        }

        private static void Case_00205()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 205,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-17,86,2,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "59e9ef58315a7b41e1a9c267b05d42542fbb0e72a388849ebb6b80ab356ecaa2");
        }

        private static void Case_00206()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 206,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,0,90,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-15,34,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,4,59,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,19,56,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,-3,19,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,0,16,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,8,80,6,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,-11,9,36,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "0698ad94f9a69dfc8c3b94919d5ec0eea42a550ebef2d720eb2e986b0ad6c0a0");
        }

        private static void Case_00207()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 207,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-2,40,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,9,94,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,8,33,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,-3,97,7,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "86cc61ebeef8ee60a8430090d510a1257455c6a0d04594269a2d92854a394cb2");
        }

        private static void Case_00208()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 208,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,13,9,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-18,64,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,11,100,38,3), new GeneratedEnemyUnit(-8,3,41,26,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 80,
                stableHash: "118ed5445a69b901ca2758fa04d3a6dd06bfc62b142f919392435157813763fc");
        }

        private static void Case_00209()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 209,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,-7,75,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,9,11,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,14,67,21,2), new GeneratedEnemyUnit(20,8,31,36,3), new GeneratedEnemyUnit(-13,-1,43,10,3), new GeneratedEnemyUnit(-19,-7,53,16,2), new GeneratedEnemyUnit(9,-4,71,3,4), new GeneratedEnemyUnit(6,-7,67,28,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "4a745980b7d245bffc8e798200c1346105502d6160ee88193542cc480fadbe46");
        }

        private static void Case_00210()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 210,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-18,72,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-18,10,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,13,6,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-19,89,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,-13,40,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-10,79,33,3), new GeneratedEnemyUnit(11,12,11,29,4), new GeneratedEnemyUnit(17,9,97,19,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "4c860939b959cdcd03c549bde3a3ce086388cfefdfb38b299298e9d22b81a5e3");
        }

        private static void Case_00211()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 211,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,4,46,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,12,93,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-12,38,1,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-10,1,38,1,4), new GeneratedEnemyUnit(10,-6,45,44,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "0ccfb0bf5ebbf15a748a5a3e661827e0c9de1d55b2bc7ff289d6322033178d90");
        }

        private static void Case_00212()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 212,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,5,76,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,-7,83,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,18,92,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,18,90,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-9,29,21,2), new GeneratedEnemyUnit(11,18,6,25,3), new GeneratedEnemyUnit(-6,-16,57,44,2), new GeneratedEnemyUnit(-14,-8,20,4,4), new GeneratedEnemyUnit(17,-13,81,26,4), new GeneratedEnemyUnit(13,-10,41,19,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "ca4f4f312fde2af2a38b45b739383a004c3f0f17e66884bc35d23ea758645c1c");
        }

        private static void Case_00213()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 213,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,2,5,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,11,74,20,4), new GeneratedEnemyUnit(6,-1,19,40,2), new GeneratedEnemyUnit(8,-19,68,44,2), new GeneratedEnemyUnit(8,-1,24,7,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "b372a4a96cbe5613f5a17f6ccda5a968a8581d871db718cc7dc80b596346d5d4");
        }

        private static void Case_00214()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 214,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,5,8,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,-16,62,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,-2,56,8,1), new GeneratedEnemyUnit(5,-16,51,45,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "7df6166c0c17277807fdab85f230b24ddb25be66a8ce1d222611e45a8d6d8fd1");
        }

        private static void Case_00215()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 215,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-2,65,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,-19,34,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-5,8,66,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,18,67,4,4), new GeneratedEnemyUnit(19,7,25,9,2), new GeneratedEnemyUnit(10,5,66,44,1), new GeneratedEnemyUnit(-7,10,62,23,1), new GeneratedEnemyUnit(1,8,8,17,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "dc49c48074afc6dc4f0977bacf219cd344610b1c62a44e3a351ec28a7651c6e8");
        }

        private static void Case_00216()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 216,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-6,76,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-13,4,9,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-19,17,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,14,77,18,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "02ac499a779a394d2499c96f106d380b7ed6b685acdbc20695d5c7a77ac948b1");
        }

        private static void Case_00217()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 217,
                posture: BotStrategicPosture.Opening,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,5,39,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,4,66,1,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-17,98,25,4), new GeneratedEnemyUnit(5,19,98,39,4), new GeneratedEnemyUnit(-6,-15,14,23,4), new GeneratedEnemyUnit(15,14,78,42,1), new GeneratedEnemyUnit(6,-17,20,49,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "932303d672663fcf529cbdeec8ac234eaa3fae09a43d4371b662de3d560ec319");
        }

        private static void Case_00218()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 218,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,19,24,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,3,5,15,4), new GeneratedEnemyUnit(19,-17,89,35,3), new GeneratedEnemyUnit(3,-1,27,2,3), new GeneratedEnemyUnit(17,12,7,47,1), new GeneratedEnemyUnit(1,18,66,50,3), new GeneratedEnemyUnit(8,17,12,14,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 100,
                stableHash: "a80f3d12830e7414f38b69810ba3b941eb2b3d3a639d2d3858aa80e200ce3c53");
        }

        private static void Case_00219()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 219,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-1,53,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,19,24,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-18,34,2,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,2,24,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-18,65,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,7,59,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,2,76,18,1), new GeneratedEnemyUnit(7,3,90,24,4), new GeneratedEnemyUnit(0,-15,65,39,2), new GeneratedEnemyUnit(-3,-9,6,50,2), new GeneratedEnemyUnit(17,11,41,33,1), new GeneratedEnemyUnit(-1,-13,73,11,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "f670c17daffe6d21902eb03c32d245dcedf637e02bb3855e5bf6483b0daf2532");
        }

        private static void Case_00220()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 220,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,7,92,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,-6,23,8,2), new GeneratedEnemyUnit(4,-2,56,44,2), new GeneratedEnemyUnit(20,20,9,35,3), new GeneratedEnemyUnit(14,-8,28,25,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "83df27fbd0c8344ce673672ec43fe0bc9c6b2c438c43c344fd94245daf29ce14");
        }

        private static void Case_00221()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 221,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,-9,67,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,0,10,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,7,33,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,2,10,32,2), new GeneratedEnemyUnit(8,-18,59,40,4), new GeneratedEnemyUnit(7,-7,68,44,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "76722ba775b18ff56c6b9c5f04cb2ccbc3d6101604c7088f4e7da57f88a58625");
        }

        private static void Case_00222()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 222,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-18,85,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-9,70,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,-1,29,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-13,30,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,5,60,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,6,65,7,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,15,45,29,4), new GeneratedEnemyUnit(15,20,30,24,4), new GeneratedEnemyUnit(-17,-8,20,42,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "99018c84a2c6cf357fb96465448f7371b6b1941778d76d8e355e39dd403db2fc");
        }

        private static void Case_00223()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 223,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,8,73,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,18,91,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-17,38,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-16,7,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,13,91,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-18,6,5,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,-19,45,6,3), new GeneratedEnemyUnit(-18,-9,10,15,1), new GeneratedEnemyUnit(-7,2,13,45,1), new GeneratedEnemyUnit(0,16,28,7,1), new GeneratedEnemyUnit(7,-8,16,40,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "7f833fe1a8d11ab1d236013e60613a4ac8d4123580a21c18b2f7e2570b3f9ed1");
        }

        private static void Case_00224()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 224,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,2,37,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,17,88,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,3,8,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,8,38,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,13,51,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-20,37,37,3), new GeneratedEnemyUnit(-1,-11,55,12,3), new GeneratedEnemyUnit(15,3,94,17,2), new GeneratedEnemyUnit(14,-4,41,31,2), new GeneratedEnemyUnit(0,-1,35,25,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 90,
                stableHash: "0fc8e462e8b6d73db36b1adb87bd746cc0e6e06d27ecf85fdcec3780277c4df2");
        }

        private static void Case_00225()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 225,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,10,70,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-3,16,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,14,18,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-13,-16,39,6,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-9,72,43,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "2b09e00d15de3a8889bbd242bd28404425aefc83b83db32b2044e01809af0538");
        }

        private static void Case_00226()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 226,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,19,84,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,13,25,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,-11,81,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,1,46,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,4,49,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,12,33,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,10,53,41,2), new GeneratedEnemyUnit(7,-15,25,7,1), new GeneratedEnemyUnit(-8,-16,100,42,4), new GeneratedEnemyUnit(20,-20,63,27,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "0a1594e5237677d4000ee18c54f498d4220221958d15f4ce411b6b0f6a073f0d");
        }

        private static void Case_00227()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 227,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-7,29,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(7,-12,82,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-5,13,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-13,75,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,10,86,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,12,99,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,2,89,1,3), new GeneratedEnemyUnit(-1,3,22,29,3), new GeneratedEnemyUnit(-4,-20,74,8,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "1bb61c5a520c2a120c455057777f4eaaad2e1afb26c2bc7edb7ea618bc024545");
        }

        private static void Case_00228()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 228,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-17,31,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,5,47,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,4,27,5,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,-20,32,34,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "86ab3228fec42931ac8765f2ef2249ab42c385cb64665bbe1a1abcbccfd01c60");
        }

        private static void Case_00229()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 229,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,-8,80,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-12,61,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-20,31,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,6,35,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(13,-4,55,30,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "c287a8c966785c5fc1c85ad5c6510c1a1a9a0e098e8ffde89c982dcba94c2a8b");
        }

        private static void Case_00230()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 230,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,13,31,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,13,96,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,7,87,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,3,56,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,17,74,6,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,18,70,25,2), new GeneratedEnemyUnit(12,2,50,49,2), new GeneratedEnemyUnit(-16,4,96,20,3), new GeneratedEnemyUnit(7,-9,73,7,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "207cb83ee036d470f2a5d43f2f0b0b90584fa9d5292f1a5dcc2858a56fced1b6");
        }

        private static void Case_00231()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 231,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-10,22,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,11,43,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,-13,82,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,7,75,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,19,37,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-1,13,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,17,88,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,9,49,13,4), new GeneratedEnemyUnit(-20,-8,90,40,4), new GeneratedEnemyUnit(0,-3,29,8,2), new GeneratedEnemyUnit(-20,-16,49,12,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "57086b0ff9e0828cadb342d4c1a3dfe0a19688ca6932d03a2d57e0459793aca5");
        }

        private static void Case_00232()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 232,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-10,70,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,15,63,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,20,97,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,10,52,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,0,37,15,4), new GeneratedEnemyUnit(5,-7,16,5,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "fd4248151c825f3ac6df7ea321ad41043b6c84072a715f79bba700f4a79afbd9");
        }

        private static void Case_00233()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 233,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-16,96,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,9,61,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,-18,68,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(18,-10,40,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,17,60,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-3,48,3,2), new GeneratedEnemyUnit(-19,3,58,12,4), new GeneratedEnemyUnit(19,-12,66,37,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "ec63f880ffcf8f44676f85c955d2f37af7ce910ba0a3484b9e029cdb745853f4");
        }

        private static void Case_00234()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 234,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-20,16,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,-10,54,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,3,83,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,-6,61,6,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,-10,79,23,3), new GeneratedEnemyUnit(-2,16,42,25,4), new GeneratedEnemyUnit(8,10,58,25,2), new GeneratedEnemyUnit(11,-8,9,17,3), new GeneratedEnemyUnit(-9,12,66,28,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "574f69ab300b9aa6259c5a06b20167ef98f81425c11094852502e5c71c8e13f8");
        }

        private static void Case_00235()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 235,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,8,58,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(18,-4,54,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,18,93,1,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,14,72,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,-5,92,5,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "eb901ff72ef638eb928f330e0bbca08b7a74725ed21e22bceccf64ae69ca0fdd");
        }

        private static void Case_00236()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 236,
                posture: BotStrategicPosture.Pressure,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,1,41,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,20,96,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-5,22,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,13,10,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,8,98,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,1,69,6,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "1121702bb76a0b6011d1722798cb62435f1c094fef8de5305a59f93a30540580");
        }

        private static void Case_00237()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 237,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-10,-19,14,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-20,65,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,17,16,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-20,32,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,9,48,1,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,13,7,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,-3,83,39,3), new GeneratedEnemyUnit(-18,16,62,15,4), new GeneratedEnemyUnit(14,7,68,8,4), new GeneratedEnemyUnit(17,-12,31,32,1), new GeneratedEnemyUnit(-3,14,12,39,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "892d8a39a4e1764f2c8e1c8081bb2da2bcc5dca0afb4ecdab891c22b1965fa47");
        }

        private static void Case_00238()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 238,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,12,62,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,4,61,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-5,54,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,6,11,5,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,19,67,39,4), new GeneratedEnemyUnit(-6,-7,66,43,3), new GeneratedEnemyUnit(-15,-1,38,40,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "5cfeb105234a7b944411470c6c3f0b70ef500e053842902854036f426d17e64c");
        }

        private static void Case_00239()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 239,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -6, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,16,41,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,6,35,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,-11,84,5,4,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "6f1b6520a297978e6d78af14874c2f11663dbeae3f619dce8ee9e2a8425e0313");
        }

        private static void Case_00240()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 240,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-7,94,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(13,1,58,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,16,66,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-13,55,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-2,-17,16,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,20,95,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,10,45,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-20,45,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-18,-15,76,40,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "c275808232d5efd3c8fb521b2f9eb54f6c3e534ef38205ddf80cd56ed4c7ff37");
        }

        private static void Case_00241()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 241,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,1,41,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,12,36,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,10,15,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,-6,23,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,6,42,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-6,7,72,6,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "48cf16a3d8fe10eaa37a25771b62919064d66bc61a90874526ad9d75911a9691");
        }

        private static void Case_00242()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 242,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-11,2,60,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-11,67,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,10,77,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-1,42,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(9,-15,61,24,1), new GeneratedEnemyUnit(10,1,12,48,2), new GeneratedEnemyUnit(9,-10,11,5,4), new GeneratedEnemyUnit(-12,-5,83,2,3), new GeneratedEnemyUnit(-7,17,58,24,3), new GeneratedEnemyUnit(14,16,87,41,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "fd09478e7ceb774e34239942fde7e6b064d010f8f81c3ee902a8d4516311d805");
        }

        private static void Case_00243()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 243,
                posture: BotStrategicPosture.Opening,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,13,6,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,3,46,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-5,26,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-7,19,87,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,15,16,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,4,42,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,2,28,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,17,87,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,16,60,9,2), new GeneratedEnemyUnit(19,0,14,39,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "75d7faf9c3834c3b310e2fdf0c4fc0667b3d1340b00d00e52625117050928a32");
        }

        private static void Case_00244()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 244,
                posture: BotStrategicPosture.Pressure,
                castleX: 4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-12,45,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,8,47,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,7,24,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,15,95,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-14,-3,51,5,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,0,90,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,5,16,2,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,13,68,43,2), new GeneratedEnemyUnit(13,-2,98,47,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "572aaec0103c08080ddc272b722ec6e9d9e9d10f7340868700c1085fd6bbb1e1");
        }

        private static void Case_00245()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 245,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,12,14,1,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-5,-1,27,7,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,15,52,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,16,94,3,2,BotUnitTacticalRole.Siege) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "89848c0e10d079613a9a5d2dd1a144dc3f467359237ff085b0e39eb2a99afc99");
        }

        private static void Case_00246()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 246,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-9,16,21,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(18,-7,36,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,4,30,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,12,43,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,1,43,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-18,78,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-13,-17,51,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,-18,15,35,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "94e484eb54bb5eb6d1ccaa81c838fd104261a367e5f1abdf4e9e129a6240e5e6");
        }

        private static void Case_00247()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 247,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,-10,5,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,6,28,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,16,9,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-6,35,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-3,35,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,10,94,16,4), new GeneratedEnemyUnit(9,14,86,12,2), new GeneratedEnemyUnit(13,7,49,15,3), new GeneratedEnemyUnit(-6,0,31,27,1), new GeneratedEnemyUnit(15,14,19,2,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "27c83d2c97f54d113dee362fa58e4e2c234c413b5e73de6a9582a9779b70698f");
        }

        private static void Case_00248()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 248,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,16,95,4,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,3,75,3,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-12,91,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,17,37,19,4), new GeneratedEnemyUnit(-11,-20,8,1,2), new GeneratedEnemyUnit(-12,17,65,37,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "59b418e64728f2cf6bd41379115f5f822f115d6fdb4bc094e502784ebe260d39");
        }

        private static void Case_00249()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 249,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,-19,28,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,7,77,6,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "e1cfea379753dade3a8068e84083aadba536ba786d75540661fb1518c0b3c891");
        }

        private static void Case_00250()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 250,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-16,96,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,12,11,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,5,73,1,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,19,59,1,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "0fd073dd23abfd0958f9cb77b8175d16ace8f31071e84e220889c0040e7d220c");
        }

        private static void Case_00251()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 251,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-6,78,7,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-2,91,4,2), new GeneratedEnemyUnit(11,19,89,19,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "336324ee0609e233ec5f1034a4ce298c8afa83ce2a0aa068dd23313f3a19ae14");
        }

        private static void Case_00252()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 252,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,16,11,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(3,20,39,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-20,72,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,11,86,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,6,92,49,1), new GeneratedEnemyUnit(15,-4,18,40,2), new GeneratedEnemyUnit(-14,-20,56,10,2), new GeneratedEnemyUnit(-3,8,65,39,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 35,
                stableHash: "7862d674fe377463cba0575a7c22c0a0769bdce77f8bb4e1a3af8c3b745f618b");
        }

        private static void Case_00253()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 253,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,-8,28,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,18,9,7,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(16,18,96,3,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-15,65,5,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-10,99,45,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "8d3a973e17ecb4f48bd13101daf3e7851809960743f9d82fa1172d019cd66191");
        }

        private static void Case_00254()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 254,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,4,81,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-8,-12,94,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,2,39,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-2,-11,75,3,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-7,17,22,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,18,81,32,2), new GeneratedEnemyUnit(11,-20,9,49,2), new GeneratedEnemyUnit(6,15,23,22,1), new GeneratedEnemyUnit(14,7,42,22,2), new GeneratedEnemyUnit(8,-12,97,18,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "1188f39b2fc53ec669cc728f569b7b44355cecc6f4fc443c1a21ebaccf8fd3f0");
        }

        private static void Case_00255()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 255,
                posture: BotStrategicPosture.Search,
                castleX: 3, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-1,19,25,1,4,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 100,
                stableHash: "fd8c8b6737e4886c94f158d3932e81f43f85a4c60e645285c47cf8a4004082f0");
        }

        private static void Case_00256()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 256,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,16,28,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,17,33,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-20,7,72,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,-16,88,42,4), new GeneratedEnemyUnit(-2,-17,35,4,4), new GeneratedEnemyUnit(-4,-16,8,27,2), new GeneratedEnemyUnit(4,-1,60,26,4), new GeneratedEnemyUnit(-6,-7,84,11,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "058134eb03704cb86dae17389058b5295c9e9db9b8fbbf8bb32fb279a934c7d0");
        }

        private static void Case_00257()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 257,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-4,30,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-6,1,45,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,-14,63,2,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,10,11,45,2), new GeneratedEnemyUnit(20,-4,91,37,3), new GeneratedEnemyUnit(-4,20,22,46,1), new GeneratedEnemyUnit(-2,-2,93,40,3), new GeneratedEnemyUnit(-15,0,13,1,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "46e3fb7e5453af9c2dfbcb9cbb8768258ebde78f93b4a56a8b57ac83ff86d231");
        }

        private static void Case_00258()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 258,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,15,92,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,-7,26,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,16,73,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-19,7,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-15,96,50,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "92dd969f2bcee74e0531cf54bd270c2f62883963468f6436c498d92b6bbeceb7");
        }

        private static void Case_00259()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 259,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,-1,96,1,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-6,5,7,3), new GeneratedEnemyUnit(-18,-4,40,4,4), new GeneratedEnemyUnit(14,-2,7,43,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "32a04e32da1e93a261346380780373bb2693f57c315e99be4567eb676b5c7178");
        }

        private static void Case_00260()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 260,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,8,47,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-11,5,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-16,81,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,17,10,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,-16,53,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-7,20,87,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-5,23,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-18,22,43,3), new GeneratedEnemyUnit(-19,20,96,30,3), new GeneratedEnemyUnit(4,-15,42,20,1), new GeneratedEnemyUnit(-17,10,44,39,2), new GeneratedEnemyUnit(15,-13,72,37,4), new GeneratedEnemyUnit(-12,17,60,10,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "458d7f2a3987528911405fca37d93e036019246759c2071e2b41d5d68a30ebab");
        }

        private static void Case_00261()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 261,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,7,17,7,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,17,65,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-2,95,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,11,83,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,11,83,4,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,-12,53,28,3), new GeneratedEnemyUnit(-5,13,88,42,4), new GeneratedEnemyUnit(7,6,73,27,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "15e7c28a8d77ee93d288fa2e9107296c2fef2aed799488e1012ae3de2d644833");
        }

        private static void Case_00262()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 262,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,17,79,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(16,-16,52,7,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-18,75,28,2), new GeneratedEnemyUnit(-11,-4,20,44,3), new GeneratedEnemyUnit(-9,2,69,39,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "72c50321c852f80e45b2c07accd6ff9b7da0926520e62d08a7746115635b67b0");
        }

        private static void Case_00263()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 263,
                posture: BotStrategicPosture.Pressure,
                castleX: -4, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,0,91,1,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,-2,13,12,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "09a209c221956f955c3e1680a7be74b84d36c5dc286fd85d50587e314afd660b");
        }

        private static void Case_00264()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 264,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,13,49,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,17,49,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-1,89,4,2), new GeneratedEnemyUnit(4,-6,43,31,1), new GeneratedEnemyUnit(9,-3,83,7,3), new GeneratedEnemyUnit(14,16,86,35,2), new GeneratedEnemyUnit(1,-15,94,45,2), new GeneratedEnemyUnit(-6,17,67,24,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "09af533f848d17c02407eeba5183b1de20f05e39c2b5d68550d5ec2c2c13e954");
        }

        private static void Case_00265()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 265,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,4,98,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-20,90,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-9,8,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(6,20,71,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,-18,8,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-16,79,16,3), new GeneratedEnemyUnit(-3,1,95,13,2), new GeneratedEnemyUnit(6,2,47,49,1), new GeneratedEnemyUnit(11,-6,71,7,1), new GeneratedEnemyUnit(-10,11,88,49,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "544131691bfd8162e1fea6c15bdfceb7991488305e2a2d72a195a564b7e3a937");
        }

        private static void Case_00266()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 266,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -4, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,-12,67,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(13,-5,78,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-18,68,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-18,-17,40,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(7,14,83,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-20,5,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,-14,32,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,8,66,1,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,18,75,43,2), new GeneratedEnemyUnit(-2,-8,55,22,1), new GeneratedEnemyUnit(-12,-11,65,24,3), new GeneratedEnemyUnit(-2,-14,93,41,4), new GeneratedEnemyUnit(9,12,31,12,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "142e73d7710ee553442003e08a0aedf48835d5c4261fbab3c60d17b3605a2780");
        }

        private static void Case_00267()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 267,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-10,8,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,9,91,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(6,4,37,6,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-7,16,16,2), new GeneratedEnemyUnit(17,-17,67,30,4), new GeneratedEnemyUnit(20,4,46,16,2), new GeneratedEnemyUnit(-17,-17,44,49,4), new GeneratedEnemyUnit(5,11,29,24,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 35,
                stableHash: "a0bf14a4a4b5b127929904f5fee5b19317f995032a3c98974a0e68b048245f5d");
        }

        private static void Case_00268()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 268,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,17,26,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,3,26,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(8,20,20,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,4,45,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,13,30,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(10,-19,89,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,8,61,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-9,10,62,20,1), new GeneratedEnemyUnit(14,-11,82,1,1), new GeneratedEnemyUnit(-11,20,23,36,1), new GeneratedEnemyUnit(-3,20,21,29,3), new GeneratedEnemyUnit(19,-10,54,38,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "8ccece03cf6f379d3bcf8cd1ec5c7799bb64913de930620f74012328ae1554a3");
        }

        private static void Case_00269()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 269,
                posture: BotStrategicPosture.Pressure,
                castleX: 1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,-19,72,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,12,41,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,1,47,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,4,27,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,9,27,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-4,93,3,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-19,86,2,2), new GeneratedEnemyUnit(3,-9,79,23,2), new GeneratedEnemyUnit(-6,-14,8,18,4), new GeneratedEnemyUnit(-3,0,8,39,3), new GeneratedEnemyUnit(-12,4,99,50,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 45,
                stableHash: "f3a066dbee3ad69fa6a23279173601c83cfb740808ba99d465827415b5c289d8");
        }

        private static void Case_00270()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 270,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-5,8,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,7,69,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,18,26,7,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,-2,12,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,-13,52,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,-3,25,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,2,81,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,-16,58,7,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,6,56,32,3), new GeneratedEnemyUnit(-18,-5,58,29,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 80,
                stableHash: "389a3dccc1ed039e5511cbff3a7d78c61f3018ce552f4f1e86c36073ea2c38be");
        }

        private static void Case_00271()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 271,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,-12,72,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-17,69,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,-18,71,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,0,79,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,9,68,28,1), new GeneratedEnemyUnit(2,-17,57,46,2), new GeneratedEnemyUnit(-11,-14,54,19,4), new GeneratedEnemyUnit(12,9,26,38,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "ff1c90d6fd2814faac09dde4d432fbc3a082f76bcb546a8e8c8bf5a83f7767d4");
        }

        private static void Case_00272()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 272,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,20,21,4,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-1,17,59,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-4,21,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(1,10,33,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,17,94,1,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-5,92,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-9,6,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,17,19,34,3), new GeneratedEnemyUnit(-4,1,51,8,4), new GeneratedEnemyUnit(-8,-19,38,27,1), new GeneratedEnemyUnit(12,6,27,38,1), new GeneratedEnemyUnit(-19,-10,24,2,1), new GeneratedEnemyUnit(20,3,66,14,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "16c062bda05f24a3503d7638ef08ceeeca1a0dfb19d09ac5f7db506f512b086d");
        }

        private static void Case_00273()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 273,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,11,20,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-7,95,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-17,16,58,2,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(0,16,53,2,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,-18,34,35,3), new GeneratedEnemyUnit(-6,4,94,35,4), new GeneratedEnemyUnit(-19,16,9,34,4), new GeneratedEnemyUnit(-17,-7,88,47,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "41992ab6b1b5b71ccade7b84622cc40b22c65a70e7d98c9aa39cc49134cdc305");
        }

        private static void Case_00274()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 274,
                posture: BotStrategicPosture.Pressure,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-11,65,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-12,-9,41,12,1), new GeneratedEnemyUnit(12,14,68,21,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "4d01ad88e53feeb472221645310c28598b270f860b4212acce73d213e42a37b9");
        }

        private static void Case_00275()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 275,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,18,61,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,16,61,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,12,95,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,13,87,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-3,10,51,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,19,9,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-9,2,62,4,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,20,99,33,3), new GeneratedEnemyUnit(5,-1,46,2,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "b12663edddc31608ad1fceb5c60ab68fea919f092aa80383d1cb8229add9f47c");
        }

        private static void Case_00276()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 276,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,8,54,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,-6,57,2,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(14,19,19,1,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "22d0fd8a1a2682e50b11e3a1b69037c35c85000b6b8bf759106f9b58bb07ba65");
        }

        private static void Case_00277()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 277,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-12,18,39,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(9,5,57,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,1,13,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(2,-15,75,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,11,98,5,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,13,57,17,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "ea05ac6623ffd7faade739fb2dc748bde10f327338137b29b1142cd948f795e0");
        }

        private static void Case_00278()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 278,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(2,17,87,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,12,6,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,12,64,3,2,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "06f0c66ca2f2fcdc4960d01ea1e78f0b19282b73d1e378657cbcd04f9fef8258");
        }

        private static void Case_00279()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 279,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,18,16,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,13,31,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,-3,82,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-15,42,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,13,12,7,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,12,23,6,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,2,44,17,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "02f6b523f196875f0fc9f305a58156933f9698d491ef825348e6d905190d3589");
        }

        private static void Case_00280()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 280,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 5, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-5,82,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(3,10,17,7,1,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-17,45,7,2), new GeneratedEnemyUnit(-20,-1,30,28,1), new GeneratedEnemyUnit(6,-12,95,33,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 45,
                stableHash: "8b352455eb5284533da8857c22fea240f4788a2a21245c81f4392af919a8c7ae");
        }

        private static void Case_00281()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 281,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,5,41,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,2,49,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-8,-9,68,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,2,69,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-8,84,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-4,22,3,1), new GeneratedEnemyUnit(5,2,97,4,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "5e3d430d6a3a0e9c93f53add5416e0840cabfebc767ed0ad8a18db6c32ad97ef");
        }

        private static void Case_00282()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 282,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(11,12,56,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(3,6,5,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-15,22,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,20,30,19,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "2a33c60f33c57459625d20c6e839b075d3df978185742797d2528bc3b842bb90");
        }

        private static void Case_00283()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 283,
                posture: BotStrategicPosture.Siege,
                castleX: -6, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,20,94,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,-18,14,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(9,-5,98,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,18,70,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-19,12,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,-14,94,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,0,35,6,3), new GeneratedEnemyUnit(13,11,57,45,3), new GeneratedEnemyUnit(17,10,44,22,2), new GeneratedEnemyUnit(-4,-10,88,5,1), new GeneratedEnemyUnit(-13,14,79,1,4), new GeneratedEnemyUnit(15,-13,68,46,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "11e1506ce09ba256439030a8adc707d9d6101af756b1e6d9cdb1052e1eb1c15e");
        }

        private static void Case_00284()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 284,
                posture: BotStrategicPosture.Pressure,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-6,57,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,17,7,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,7,71,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,11,99,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,7,51,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-16,-3,16,5,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,19,81,7,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-15,71,18,1), new GeneratedEnemyUnit(-2,-11,24,28,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "4c1c186751f6d21e8f94e3c66f9a6922f369921f56865aad6dd9d59dff1da7ed");
        }

        private static void Case_00285()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 285,
                posture: BotStrategicPosture.Search,
                castleX: 0, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,18,6,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(13,9,65,5,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,10,51,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(1,-19,28,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,17,92,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-10,60,5,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,-5,18,29,1), new GeneratedEnemyUnit(2,-16,69,11,1), new GeneratedEnemyUnit(-2,14,72,21,4), new GeneratedEnemyUnit(-1,-14,75,19,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 55,
                stableHash: "0324c180918c537cc1f297c66ee00caf2fcbd50774b45baccaabf7d9bdb03502");
        }

        private static void Case_00286()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 286,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 2, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,0,22,3,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,18,96,42,1), new GeneratedEnemyUnit(20,8,97,34,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "5aed14efe9a96131f6162959ea1e2e1536b3d2955b197476abce59173899df63");
        }

        private static void Case_00287()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 287,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,12,55,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,8,38,3,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(0,-20,40,11,4), new GeneratedEnemyUnit(-8,5,35,21,4), new GeneratedEnemyUnit(13,13,26,36,2), new GeneratedEnemyUnit(-10,-19,40,43,2), new GeneratedEnemyUnit(-10,-7,10,33,1), new GeneratedEnemyUnit(-4,9,92,34,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "30ed9c9b01765080e5b2094528e077cf4f853ccbce6d86c676eaa7780a529576");
        }

        private static void Case_00288()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 288,
                posture: BotStrategicPosture.Opening,
                castleX: -3, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,-10,75,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,17,72,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-1,8,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(13,-17,58,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,13,98,23,2), new GeneratedEnemyUnit(18,0,53,5,2), new GeneratedEnemyUnit(13,9,7,13,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "984940bb474f9190225b2ba900d477afb2c0c0ee34e967449c60f57b59ebe95e");
        }

        private static void Case_00289()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 289,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,3,85,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(19,7,46,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-5,3,35,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-18,30,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,-8,18,4,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,4,68,8,4), new GeneratedEnemyUnit(-11,-14,13,31,2), new GeneratedEnemyUnit(-8,3,25,38,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "2fae341624208a72464369ab8c5279f45da186b7efc6696fc66f272a8244bbbd");
        }

        private static void Case_00290()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 290,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(7,8,84,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,20,72,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,12,82,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,-3,97,3,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-20,20,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-17,100,1,4,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 30,
                stableHash: "abcaadddabda183807adba2d04e94a11c34012613b1c49c6cc0833bb14618af9");
        }

        private static void Case_00291()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 291,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-11,62,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,18,19,7,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,15,31,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,16,64,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(19,10,49,6,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(1,-12,64,5,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(19,3,71,43,3), new GeneratedEnemyUnit(-14,4,33,11,3), new GeneratedEnemyUnit(5,-4,22,7,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "421cdf4a70e6f7e15fdec200da5cb685ded8fab822c9f6f25db50c1ac8834246");
        }

        private static void Case_00292()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 292,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,11,12,5,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(17,6,36,2,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-10,40,43,2), new GeneratedEnemyUnit(-18,-12,75,20,1), new GeneratedEnemyUnit(-12,10,69,6,1), new GeneratedEnemyUnit(17,2,80,9,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "fe86c9e72fabcb97502cc9f119cf34fa6a0746ae3215cfcda6c58f2c2f88de03");
        }

        private static void Case_00293()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 293,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-4,14,49,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-15,4,80,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,14,82,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,-10,55,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,4,61,2,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-15,-12,7,5,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,11,68,12,4), new GeneratedEnemyUnit(7,20,64,13,3), new GeneratedEnemyUnit(12,4,50,34,3), new GeneratedEnemyUnit(-3,19,22,50,1), new GeneratedEnemyUnit(3,-9,57,29,1), new GeneratedEnemyUnit(-15,8,32,8,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 90,
                stableHash: "86886df2ebdd484d1d2ec951fa1ff5983dc81cf72a9f0aaff89154154b34188b");
        }

        private static void Case_00294()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 294,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-7,89,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(15,-19,92,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(14,4,74,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-16,19,86,3,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(16,-3,66,32,2), new GeneratedEnemyUnit(-12,-6,19,7,2), new GeneratedEnemyUnit(0,3,96,21,2), new GeneratedEnemyUnit(0,-19,27,2,3), new GeneratedEnemyUnit(17,-5,61,20,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 90,
                stableHash: "a709ab32f4ee16334628046d9f1bc75be77959a0876ea41e08df72577876df29");
        }

        private static void Case_00295()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 295,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,11,47,6,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,-1,64,42,1), new GeneratedEnemyUnit(-8,11,8,11,4), new GeneratedEnemyUnit(4,11,35,22,2), new GeneratedEnemyUnit(6,17,42,10,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "bbffcba4f7e229e9622581a9ba1b2ca79bb4134a66b69d203c05e0a2612cf430");
        }

        private static void Case_00296()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 296,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 0, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-17,-10,54,3,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,-11,100,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-2,-17,42,5,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(15,18,66,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-3,41,4,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(20,-15,82,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,-1,57,13,3), new GeneratedEnemyUnit(9,-7,85,13,2), new GeneratedEnemyUnit(3,17,44,28,2), new GeneratedEnemyUnit(19,-6,13,11,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "bdb941c3ace0f75cd977d06a1816a9be0d26d0a37a9fddbe7c37bdfc41a9ec1b");
        }

        private static void Case_00297()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 297,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-14,85,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,7,100,28,4), new GeneratedEnemyUnit(-1,-12,98,7,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "5ca80a2519f2b13715aa93bd25fae732f7ecdcccb4d36094e3581e003ca4f4fc");
        }

        private static void Case_00298()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 298,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-6,-14,22,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,18,45,6,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-18,-19,46,4,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(8,-9,37,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-9,-5,86,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-15,79,29,3), new GeneratedEnemyUnit(2,-18,77,43,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "bfac778872bfc5ed6c94ec1cf64c686cbef58fef6c7a32535a5d65c98c059a73");
        }

        private static void Case_00299()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 299,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,20,81,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,20,90,4,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "ffe98305237862f22a0d846c13580788c58fd9b6152340697b0393a9dd9e3afd");
        }

        private static void Case_00300()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 300,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,5,95,2,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(19,-11,61,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,-8,64,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,3,100,5,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-17,99,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-16,-10,57,3,1,BotUnitTacticalRole.Ranged) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "cf085a702ec61e561779d27c43fb1eb27626fdcde0c0be55e02b7169dde06aed");
        }

        private static void Case_00301()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 301,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,7,86,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-16,3,30,7,1,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(20,2,58,5,4), new GeneratedEnemyUnit(11,19,98,21,3), new GeneratedEnemyUnit(11,1,97,4,1), new GeneratedEnemyUnit(15,18,94,5,2), new GeneratedEnemyUnit(16,6,9,3,4), new GeneratedEnemyUnit(11,5,38,23,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 5,
                expectedHomeReservePercent: 50,
                stableHash: "9974b54d50f3dfb785e4caa6a2dd61932d37e208c6a5f5c4aab297908fbff502");
        }

        private static void Case_00302()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 302,
                posture: BotStrategicPosture.Search,
                castleX: 1, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-18,-11,66,3,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-10,17,70,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-11,2,41,26,4), new GeneratedEnemyUnit(-11,-16,38,28,3), new GeneratedEnemyUnit(-3,-19,44,49,3), new GeneratedEnemyUnit(8,15,13,41,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "ae0fde9457dcd4aa723731d47379bc2b8d73d39bb48e7cd95729174f6d200932");
        }

        private static void Case_00303()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 303,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,12,83,5,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-4,7,97,41,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "553eee0c101f1bc3c8de3cc1d1ece907910683967d08d6a0920cfa9df8e52e3d");
        }

        private static void Case_00304()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 304,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,6,59,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-17,27,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,5,24,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-2,-18,25,6,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(7,17,11,9,1), new GeneratedEnemyUnit(-9,7,55,7,4), new GeneratedEnemyUnit(-17,-10,41,26,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 40,
                stableHash: "f71be45a58a16ce27007e87de456008f3ba78b115a0744546ae227a1c6e0e0a8");
        }

        private static void Case_00305()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 305,
                posture: BotStrategicPosture.Siege,
                castleX: -3, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-15,88,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(16,14,30,6,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(20,-8,48,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,-4,31,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-2,65,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-4,-20,35,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,14,78,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,8,36,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-20,2,17,40,3), new GeneratedEnemyUnit(1,3,81,37,2), new GeneratedEnemyUnit(8,3,100,12,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "90d206cab3667c7d766618ed532b72e72499d7c148665a0db2f7fd14ea47b0ad");
        }

        private static void Case_00306()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 306,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,0,22,4,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(5,14,100,5,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-11,12,61,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,9,63,6,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,12,68,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,-14,30,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-3,-12,49,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,17,22,46,1), new GeneratedEnemyUnit(-13,-19,17,1,2), new GeneratedEnemyUnit(18,14,13,18,1), new GeneratedEnemyUnit(17,10,14,30,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "010557def477e283fbcaae34d76aa0ff49a2ab0fc712511c926a3ba6e5fd024a");
        }

        private static void Case_00307()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 307,
                posture: BotStrategicPosture.Search,
                castleX: -5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-10,10,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,19,38,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-19,20,41,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-15,-2,81,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-4,47,5,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-8,66,7,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-16,20,36,39,1), new GeneratedEnemyUnit(18,-10,18,33,4), new GeneratedEnemyUnit(18,-7,74,36,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 50,
                stableHash: "262872323b1686a6a2f6bc5edc2feadc42eb65b71b605437a3861871873ebfa7");
        }

        private static void Case_00308()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 308,
                posture: BotStrategicPosture.Siege,
                castleX: 4, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,5,7,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(11,2,26,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-3,5,88,34,2), new GeneratedEnemyUnit(-9,-8,38,36,4), new GeneratedEnemyUnit(0,9,93,26,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "66a3722d87440dd8af6e8002f35a71ec2699c81b6a9a2cf782072b2749c74a7e");
        }

        private static void Case_00309()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 309,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-5,86,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,6,60,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-4,-10,24,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,-19,54,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,19,68,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,1,38,5,3,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 20,
                stableHash: "687737844225bcac8fe1f4facdb35d614d72f39c1f1aed3b33c2d0f819c72219");
        }

        private static void Case_00310()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 310,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,12,92,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(15,-7,31,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,-4,39,3,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 75,
                stableHash: "9dcd22db827376de5e35004e571070564c8df5c7dd92b7e1f91f7d2fe61fc1c4");
        }

        private static void Case_00311()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 311,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(19,20,53,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-5,51,6,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,10,77,1,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-14,39,32,4), new GeneratedEnemyUnit(4,2,96,6,4), new GeneratedEnemyUnit(14,19,71,34,4), new GeneratedEnemyUnit(14,0,65,25,3), new GeneratedEnemyUnit(15,7,23,44,1), new GeneratedEnemyUnit(-11,-3,56,33,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 50,
                stableHash: "2601216ac80cd120c6db6119aa2c634dd2bd062f4ca34ecebb943ac96d0ff955");
        }

        private static void Case_00312()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 312,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -4, castleY: -5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,6,74,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(10,-6,73,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,14,81,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-10,-2,44,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-1,33,4,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,10,68,28,2), new GeneratedEnemyUnit(8,-6,35,35,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "026d164595acb683fec69982903fce395d6aa276c97c194c310d12da4bf4e0bc");
        }

        private static void Case_00313()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 313,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-5,9,14,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,-18,63,3,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,16,7,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-12,-18,48,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(1,-18,10,26,1), new GeneratedEnemyUnit(-6,4,60,35,3), new GeneratedEnemyUnit(7,-11,31,38,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "58be753cf63d4aafa926026edf51b6b25231467ca7cd5a9a026c29ac875fee9c");
        }

        private static void Case_00314()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 314,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(20,10,74,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-4,50,4,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,3,42,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,-19,84,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-19,85,1,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-18,3,13,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-1,12,51,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(17,-8,82,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-20,90,5,2), new GeneratedEnemyUnit(16,-16,85,45,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 30,
                stableHash: "54a852e4212374821b5fea0e46e603205d0135cd6ab3cab37fe3b14d5a0c9763");
        }

        private static void Case_00315()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 315,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 6, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,4,52,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,-12,9,7,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,6,55,6,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,11,22,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,19,24,1,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-10,28,12,1), new GeneratedEnemyUnit(-1,13,64,45,1), new GeneratedEnemyUnit(11,0,18,26,3), new GeneratedEnemyUnit(-14,11,40,1,4), new GeneratedEnemyUnit(-17,-2,67,43,4), new GeneratedEnemyUnit(18,-16,79,2,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "41ebbf1af31bcae97771b52008e21d22191e81c0fd5e9a70cb40c1479de86757");
        }

        private static void Case_00316()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 316,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,-2,72,6,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-8,64,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,4,50,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,12,85,45,1), new GeneratedEnemyUnit(9,3,20,12,1), new GeneratedEnemyUnit(-18,-11,16,2,4), new GeneratedEnemyUnit(6,7,73,30,3), new GeneratedEnemyUnit(-8,-17,8,38,3), new GeneratedEnemyUnit(-10,20,6,34,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 4,
                expectedHomeReservePercent: 40,
                stableHash: "b7bc44006cf0dd4967bebf53839ae0e9e8309bae0c7c5eb82f4499b311b90a85");
        }

        private static void Case_00317()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 317,
                posture: BotStrategicPosture.Opening,
                castleX: -4, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-8,18,7,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(12,15,36,18,4), new GeneratedEnemyUnit(-17,-18,14,36,4), new GeneratedEnemyUnit(4,-13,93,11,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 100,
                stableHash: "49a97183171bfa80099082975d7575468a2421bf463efa7b11c2198707acb910");
        }

        private static void Case_00318()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 318,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -5, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,-17,50,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-7,19,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-8,9,36,6,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-19,46,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(5,9,74,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-17,23,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(3,-4,78,1,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(11,-13,96,4,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-2,-2,10,7,1), new GeneratedEnemyUnit(-14,-12,73,1,4), new GeneratedEnemyUnit(-3,2,58,38,4), new GeneratedEnemyUnit(-13,16,82,21,2), new GeneratedEnemyUnit(9,-10,83,6,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 50,
                stableHash: "4a5a4cb7dd4ced0fe8a881c3dc1773f40a81f8f11c8aab43730763b420cba156");
        }

        private static void Case_00319()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 319,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,3,55,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,13,65,3,2,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-17,20,20,31,3), new GeneratedEnemyUnit(-17,4,20,7,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "7853b46617ab743b3a37e0d45bf8d5ea41e21abe95f63cbaa354fb1b32333bff");
        }

        private static void Case_00320()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 320,
                posture: BotStrategicPosture.Pressure,
                castleX: -3, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,12,13,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(1,-5,5,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(12,-15,89,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,10,55,1,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-10,-20,69,4,3,BotUnitTacticalRole.FastScout) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "8dbffdb42ea8dba0ba348267e6e4fbda374519eb874e656652bc49c9c81b3ed5");
        }

        private static void Case_00321()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 321,
                posture: BotStrategicPosture.Siege,
                castleX: -4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(3,-6,12,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-1,18,81,7,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,3,84,5,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(14,3,7,7,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,18,13,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-20,0,70,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(3,8,41,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,11,70,5,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,5,11,3,4), new GeneratedEnemyUnit(15,-12,15,34,4), new GeneratedEnemyUnit(5,-18,97,49,1), new GeneratedEnemyUnit(-11,-5,76,25,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "a5f13797f1880afcda062c408d1fcee6661106ce145bd38543d476df98713cb2");
        }

        private static void Case_00322()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 322,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -1, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,15,42,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,12,29,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(8,-7,49,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(11,5,98,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-13,60,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,3,25,5,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,9,89,38,3), new GeneratedEnemyUnit(-4,-5,86,17,1), new GeneratedEnemyUnit(-15,7,9,32,4), new GeneratedEnemyUnit(-2,-20,20,39,3), new GeneratedEnemyUnit(-13,-8,70,32,4) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "22cd1f9f6d8c5217fce7df83f2553be98b09d3952f177c2fcef13902f185d525");
        }

        private static void Case_00323()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 323,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(12,-13,46,5,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(2,-8,36,6,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(10,-18,84,1,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-20,14,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,-13,38,7,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,20,76,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,9,74,4,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-12,-18,82,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-16,14,44,3), new GeneratedEnemyUnit(-14,-20,25,43,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "f3f92ade0e34949e7748d8490edb3f0ec79ec66b35e4312918c00de90f7cf0cd");
        }

        private static void Case_00324()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 324,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-17,53,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-6,-13,69,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,11,23,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-17,1,65,1,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,-16,63,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(5,-15,63,4,1,BotUnitTacticalRole.Frontline) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "d4df04daa8ca77b82361e6cdc3a2aacf1a7cf8c63780cba20a4040b8e45e354c");
        }

        private static void Case_00325()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 325,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-19,8,32,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,-17,63,3,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-20,-6,81,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,4,73,4,2), new GeneratedEnemyUnit(4,5,88,3,2), new GeneratedEnemyUnit(-5,-1,77,36,1) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 35,
                stableHash: "05d3cf7ef9870351aa6f2cbe2b0c34373ddf53b36ec552dfb0960dbf7e0c94c8");
        }

        private static void Case_00326()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 326,
                posture: BotStrategicPosture.Pressure,
                castleX: 0, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(8,15,14,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-5,14,39,6,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(18,19,66,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-10,6,7,5,1,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(4,4,11,24,2), new GeneratedEnemyUnit(8,-14,42,16,3), new GeneratedEnemyUnit(12,-17,49,19,4), new GeneratedEnemyUnit(-11,18,53,48,1), new GeneratedEnemyUnit(4,-12,36,29,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "80225dc3b33099633fcd2de8beeabe7b31f559d563dd4e0b57087ee6d3066b1a");
        }

        private static void Case_00327()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 327,
                posture: BotStrategicPosture.Opening,
                castleX: 1, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,4,60,4,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,17,33,2,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,-14,36,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,5,12,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-13,75,2,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,-10,80,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-14,4,27,1,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(7,-7,81,4,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,18,24,27,4), new GeneratedEnemyUnit(-3,-5,60,4,4), new GeneratedEnemyUnit(10,-11,13,6,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "36eec2da70eb37d060f57275dd6c2f8605555ca77b413e522639d384314d247b");
        }

        private static void Case_00328()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 328,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,17,29,4,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-7,71,5,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(17,-6,75,48,1), new GeneratedEnemyUnit(-4,-19,87,18,3), new GeneratedEnemyUnit(-12,18,6,1,2), new GeneratedEnemyUnit(-4,9,67,32,3), new GeneratedEnemyUnit(10,6,21,20,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "e45e42e9f7657030ccc1e3c6560cd58b7bacb698701af0679ee18ad57f60416b");
        }

        private static void Case_00329()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 329,
                posture: BotStrategicPosture.Opening,
                castleX: 5, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-20,28,6,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(3,-10,9,13,2), new GeneratedEnemyUnit(11,15,19,5,4), new GeneratedEnemyUnit(-13,1,94,18,2), new GeneratedEnemyUnit(10,12,52,1,2), new GeneratedEnemyUnit(5,17,43,27,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 100,
                stableHash: "288befaf0bc955dc95f6147fc37474136322a107a639290264b3befebbb9ee2f");
        }

        private static void Case_00330()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 330,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: 4, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,17,92,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-8,-15,53,7,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(19,20,87,1,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-9,83,20,2), new GeneratedEnemyUnit(11,1,35,25,2), new GeneratedEnemyUnit(-11,-17,67,30,3) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 85,
                stableHash: "17e5eced369bc2d5c24fbf430ead261e247d0751f3d44b63bc6bf0883cfbf3f9");
        }

        private static void Case_00331()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 331,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-2,15,8,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,0,24,6,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,8,56,44,2), new GeneratedEnemyUnit(-4,-14,87,15,3), new GeneratedEnemyUnit(-5,-17,100,7,4), new GeneratedEnemyUnit(-2,-11,90,45,2), new GeneratedEnemyUnit(-20,-9,42,16,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "f051e544bc8909067f90faae58bd420f6ffcb80e7c5eb6475fac581e39025ae7");
        }

        private static void Case_00332()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 332,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-14,15,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,12,19,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,-10,74,6,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(14,-15,48,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(7,-10,73,7,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,14,50,5,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(4,2,34,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(20,15,88,2,1,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: true,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 35,
                stableHash: "a89d4d6791918d93f1256c5c54c5b91e38ad6fd2c36447dd9f480691ca9b1d4e");
        }

        private static void Case_00333()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 333,
                posture: BotStrategicPosture.Opening,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(17,11,63,5,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-1,-2,64,3,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,-20,59,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-7,-20,56,4,1,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-7,50,28,3), new GeneratedEnemyUnit(8,-7,89,19,2), new GeneratedEnemyUnit(7,8,61,1,1), new GeneratedEnemyUnit(0,-9,18,12,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 50,
                stableHash: "5f999981b55a92feaefeeedf12c32a113b8ccbb62fe641f60219d0ef477b0c38");
        }

        private static void Case_00334()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 334,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: 2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(13,5,65,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(6,2,88,7,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(8,20,71,5,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-15,13,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-3,72,2,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-12,-11,16,6,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(10,1,70,5,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-14,-20,99,2,2,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,5,25,10,4), new GeneratedEnemyUnit(16,-5,86,24,3), new GeneratedEnemyUnit(18,4,95,22,4), new GeneratedEnemyUnit(-4,0,45,40,1), new GeneratedEnemyUnit(19,5,80,1,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "4e6b1bf19c9189d2e83ee26b5e4e15a2c1fe2304fc19a8ed202ed4f95bd96421");
        }

        private static void Case_00335()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 335,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(1,-8,26,3,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(17,-14,43,7,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,2,13,1,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-14,9,29,2,2), new GeneratedEnemyUnit(-20,6,14,29,3), new GeneratedEnemyUnit(-12,-19,89,12,1), new GeneratedEnemyUnit(-13,1,44,9,4), new GeneratedEnemyUnit(-1,16,42,39,1), new GeneratedEnemyUnit(15,-11,75,48,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "52b012b4a13016cd11d2e04fd61b8be9e75943a5e2ee153187d62c1e3adc05b2");
        }

        private static void Case_00336()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 336,
                posture: BotStrategicPosture.Siege,
                castleX: -2, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-14,19,89,3,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,3,24,7,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(2,-2,61,39,4), new GeneratedEnemyUnit(14,-7,73,7,2), new GeneratedEnemyUnit(18,5,45,43,3) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "39b418b621fddf6bec2ae6ac95ef405a56397a5f89d6ab7364ec30eb06e6d569");
        }

        private static void Case_00337()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 337,
                posture: BotStrategicPosture.Search,
                castleX: -3, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,3,67,6,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(15,-4,73,7,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(6,-9,20,23,3), new GeneratedEnemyUnit(13,-17,41,19,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 45,
                stableHash: "fb8d21ba130c298aa83035e70313392afd4d4b327d4a99cec5397df36001ff65");
        }

        private static void Case_00338()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 338,
                posture: BotStrategicPosture.Siege,
                castleX: 6, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,-20,77,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-16,88,1,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,18,33,6,2,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,3,83,25,3), new GeneratedEnemyUnit(-20,0,56,5,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 30,
                stableHash: "0b842d48258dfd2e8378c879abff97f056fc00cb266c64ad82e079359704e7bb");
        }

        private static void Case_00339()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 339,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -1, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-2,7,4,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(5,-10,52,6,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(10,-17,74,4,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-11,-13,23,1,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,18,12,47,4), new GeneratedEnemyUnit(-4,1,55,5,4), new GeneratedEnemyUnit(-2,-15,47,49,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 90,
                stableHash: "fe87ffad114a234a778e135c4af28d0c4f46a7be73628d70a97f8e580d5995fa");
        }

        private static void Case_00340()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 340,
                posture: BotStrategicPosture.Siege,
                castleX: 3, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-20,-10,72,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-1,5,49,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,18,64,2,4,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-13,-9,43,48,1) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 20,
                stableHash: "12a5728afaf917f5f76fb17b4dec0c238ee34b1b863bd26cfab9c1be027df762");
        }

        private static void Case_00341()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 341,
                posture: BotStrategicPosture.Opening,
                castleX: 4, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(18,-15,11,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(4,18,11,5,3,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(6,8,45,6,3,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(18,1,49,13,1), new GeneratedEnemyUnit(-6,7,39,43,4) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "e632b9ed71046cdd912d7063cf5444ff72c6f1089b540d2b689b5ea743df6155");
        }

        private static void Case_00342()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 342,
                posture: BotStrategicPosture.Opening,
                castleX: -2, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-7,3,20,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,3,47,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(11,-11,63,1,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-19,24,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-11,-3,32,4,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-3,8,85,1,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(13,-14,75,3,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,1,26,45,4), new GeneratedEnemyUnit(11,-7,98,41,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "76bdc06637bcbc581ff5baa38dc0c53168b93f24828d7c523a9611e8157d829a");
        }

        private static void Case_00343()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 343,
                posture: BotStrategicPosture.Siege,
                castleX: 1, castleY: -4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(14,-7,78,2,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-8,-14,36,3,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-14,18,80,4,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(16,-14,9,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-19,-7,23,31,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "055b2337c8baabb6e0533c79edebe811e81402569f57f1c1eca6e1e10c0c9f7f");
        }

        private static void Case_00344()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 344,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: 3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,-11,28,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(0,-15,31,2,3,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-13,24,4,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(8,9,84,4,4,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(5,6,34,25,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "01c28196939145971c8fcc81bd4b80c66b9e1b7ae20ccdc1d4c73864b78c2917");
        }

        private static void Case_00345()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 345,
                posture: BotStrategicPosture.Pressure,
                castleX: 6, castleY: 1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(5,13,100,5,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-20,-8,17,5,4,BotUnitTacticalRole.Support) },
                visible: System.Array.Empty<GeneratedEnemyUnit>(),
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 0,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: -1,
                expectedHomeReservePercent: 25,
                stableHash: "33b604d3855f2f26bd7e83b8390f7fc1599d42f318988de07bf3b37b2d237a09");
        }

        private static void Case_00346()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 346,
                posture: BotStrategicPosture.Pressure,
                castleX: -6, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-8,-12,94,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(2,-18,33,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-14,-10,5,1,3,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,20,49,4,4) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "c08324e320957e531a9ecd90ab08cc489791e6ce571f779b1974332af8d8b589");
        }

        private static void Case_00347()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 347,
                posture: BotStrategicPosture.Siege,
                castleX: 5, castleY: 0,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(16,13,72,6,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-20,4,28,3,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(14,13,51,5,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,13,9,4,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,-6,11,6,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 25,
                stableHash: "6c09ed7db8c013ba3996d98d51af627dce7f84dcecc94beccb34db02d7c3a5e1");
        }

        private static void Case_00348()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 348,
                posture: BotStrategicPosture.Siege,
                castleX: -1, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-15,17,53,2,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-3,11,81,2,2,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-2,-1,60,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-18,-12,31,3,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(16,0,67,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-5,7,7,5,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(2,-13,70,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(5,11,50,2,1,BotUnitTacticalRole.Siege) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-1,-1,57,8,2), new GeneratedEnemyUnit(14,14,71,8,3), new GeneratedEnemyUnit(5,-4,27,42,3), new GeneratedEnemyUnit(-10,11,31,46,4), new GeneratedEnemyUnit(4,18,45,40,4) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 40,
                stableHash: "4cb7bbe36b5dc365e32862257be19bb2046949a4bf5b8ea0f91b4a2ff57095f5");
        }

        private static void Case_00349()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 349,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -2, castleY: -3,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-16,6,77,6,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(9,-12,78,2,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-17,-17,85,2,1,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-10,13,20,4,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,-14,18,6,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-4,-3,78,2,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(4,1,44,3,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(12,17,66,5,3,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-6,-14,7,4,3), new GeneratedEnemyUnit(19,-6,15,38,1), new GeneratedEnemyUnit(-5,6,92,15,2), new GeneratedEnemyUnit(-17,-18,47,44,4), new GeneratedEnemyUnit(-10,10,6,30,1), new GeneratedEnemyUnit(5,-13,82,10,1) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 6,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 3,
                expectedHomeReservePercent: 50,
                stableHash: "ceb4c21a6414c3b6e7b68f312ed44d872341a8d5ae3b67e0f9d6442852e5b78d");
        }

        private static void Case_00350()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 350,
                posture: BotStrategicPosture.Opening,
                castleX: -1, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,17,57,4,1,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(17,14,61,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(6,9,94,4,3,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(12,-1,79,7,3,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(8,-6,28,4,4), new GeneratedEnemyUnit(19,-4,97,24,2), new GeneratedEnemyUnit(6,13,63,46,4), new GeneratedEnemyUnit(19,19,91,6,2), new GeneratedEnemyUnit(-19,9,93,16,2) },
                hiddenEnemyCount: 0,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "8c4cfdb09b3a4bdc43644cb3d8cb79dd7a008a7d0209f7055cceca6860aba893");
        }

        private static void Case_00351()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 351,
                posture: BotStrategicPosture.Pressure,
                castleX: 2, castleY: 6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,0,90,2,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-11,-6,96,7,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(9,14,93,5,4,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,7,55,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-17,15,11,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-6,8,36,3,4,BotUnitTacticalRole.Ranged) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(11,13,84,11,3), new GeneratedEnemyUnit(19,-9,19,18,4), new GeneratedEnemyUnit(17,-20,5,4,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 40,
                stableHash: "a3c1a86e56c758d35b379eeaba6339825ac7f2c6913847ffe2d0306d1a3fca6f");
        }

        private static void Case_00352()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 352,
                posture: BotStrategicPosture.Pressure,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-9,24,4,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(19,4,62,3,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-3,-17,56,1,4,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(12,4,14,4,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-9,-20,65,1,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-8,-3,54,42,4), new GeneratedEnemyUnit(-6,-20,43,25,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "a8968ec1337c6a379bf6c40585cb302698a56ca450a313de9f00dcc0f28a1af3");
        }

        private static void Case_00353()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 353,
                posture: BotStrategicPosture.EmergencyDefense,
                castleX: -6, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(0,-17,45,7,2,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-7,4,99,1,4,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-6,6,17,5,1,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-9,17,96,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,-6,98,49,2), new GeneratedEnemyUnit(20,15,48,46,3), new GeneratedEnemyUnit(2,7,73,50,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: true,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 90,
                stableHash: "ac73b81da4a4c54bb446c1f2dd2f7c120752d834215484b0fcfb1b4d4658ee10");
        }

        private static void Case_00354()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 354,
                posture: BotStrategicPosture.Search,
                castleX: -2, castleY: -2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(4,-11,41,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-13,90,3,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(7,-17,41,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-4,13,26,7,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-8,64,4,1,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(0,18,12,3,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-17,-15,61,3,4,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-15,14,65,2,4,BotUnitTacticalRole.Support) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-7,16,71,13,2) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 1,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 40,
                stableHash: "784009780f30e8f612c93a0667fe8655c5d3f628ee387230c3d302e35738cf0b");
        }

        private static void Case_00355()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 355,
                posture: BotStrategicPosture.Pressure,
                castleX: 3, castleY: 5,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(-3,-3,48,6,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(-4,12,32,1,2,BotUnitTacticalRole.Ranged), new GeneratedOwnUnit(-19,8,87,7,4,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(2,10,78,2,3,BotUnitTacticalRole.Siege), new GeneratedOwnUnit(-19,-14,91,1,4,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-5,2,13,29,3), new GeneratedEnemyUnit(-16,-10,13,28,1) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 2,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 0,
                expectedHomeReservePercent: 35,
                stableHash: "ea1fe614aff68ba5b22f79ae87bf4f968f2f02a6df07d9907fa9af0a2a58c61c");
        }

        private static void Case_00356()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 356,
                posture: BotStrategicPosture.Siege,
                castleX: -5, castleY: -6,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(9,10,38,2,3,BotUnitTacticalRole.Support), new GeneratedOwnUnit(9,-3,75,1,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(0,-10,30,5,2,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(14,-18,85,49,1), new GeneratedEnemyUnit(14,-19,7,41,4), new GeneratedEnemyUnit(-17,10,69,43,2) },
                hiddenEnemyCount: 3,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 1,
                expectedHomeReservePercent: 35,
                stableHash: "ba45f15a83eb257dfd055cf9a570a457ea32be08a1f900ff22bcee3b0c7bcd45");
        }

        private static void Case_00357()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 357,
                posture: BotStrategicPosture.ArmyBuildUp,
                castleX: -3, castleY: 4,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(10,-10,41,1,2,BotUnitTacticalRole.FastScout), new GeneratedOwnUnit(4,12,86,7,2,BotUnitTacticalRole.Frontline), new GeneratedOwnUnit(-19,7,27,2,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(12,0,64,3,3,BotUnitTacticalRole.FastScout) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,15,16,31,3), new GeneratedEnemyUnit(-12,-15,59,12,3), new GeneratedEnemyUnit(-14,1,20,49,3) },
                hiddenEnemyCount: 1,
                expectedVisibleTargetCount: 3,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 45,
                stableHash: "b6f5f2f3df4ef54812161480e3e0318043a371f76b0d8a206c8e6f2909cd436e");
        }

        private static void Case_00358()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 358,
                posture: BotStrategicPosture.Search,
                castleX: -1, castleY: 2,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(6,6,27,1,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(11,-3,85,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(-16,20,54,7,1,BotUnitTacticalRole.Support), new GeneratedOwnUnit(20,-3,93,5,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(15,5,90,4,2), new GeneratedEnemyUnit(-10,-17,97,18,2), new GeneratedEnemyUnit(-6,3,47,47,2), new GeneratedEnemyUnit(-17,-14,89,48,4), new GeneratedEnemyUnit(0,14,58,34,2) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 5,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: true,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 55,
                stableHash: "961e2800b2713c45d5253022bb37b27c16aef01dc4ddf5b81a673faeca430e01");
        }

        private static void Case_00359()
        {
            BotGeneratedRegressionKernel.Validate(
                caseId: 359,
                posture: BotStrategicPosture.Siege,
                castleX: 0, castleY: -1,
                own: new GeneratedOwnUnit[] { new GeneratedOwnUnit(15,19,85,2,2,BotUnitTacticalRole.Frontline) },
                visible: new GeneratedEnemyUnit[] { new GeneratedEnemyUnit(-15,20,54,1,1), new GeneratedEnemyUnit(5,1,9,2,1), new GeneratedEnemyUnit(9,18,44,27,3), new GeneratedEnemyUnit(-18,10,47,2,3) },
                hiddenEnemyCount: 2,
                expectedVisibleTargetCount: 4,
                expectedEmergencyBias: false,
                expectedFrontierPreference: false,
                expectedAnyRetreatCandidate: false,
                expectedBestVisibleTargetIndex: 2,
                expectedHomeReservePercent: 100,
                stableHash: "e6d6bafb26cd781bb643ac54b9761f22469ef99ccbc39bb66bbbcdc26e118f7c");
        }

    }
}
