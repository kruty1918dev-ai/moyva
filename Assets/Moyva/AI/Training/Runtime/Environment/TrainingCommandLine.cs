using System;
using System.Globalization;

namespace Kruty1918.Moyva.AI.Training
{
    public static class TrainingCommandLine
    {
        public static void Apply(TrainingConfig config, string[] args, bool batchMode)
        {
            config.presentationMode = TrainingPresentationModeResolver.Resolve(config, batchMode, args);
            for (int i = 0; i < args.Length; i++)
            {
                string flag = args[i];
                if (flag != "-moyvaSeed" && flag != "-moyvaWorldSize" && flag != "-moyvaCurriculumStage"
                    && flag != "-moyvaTrainingTimeScale" && flag != "-moyvaEpisodeDecisions") continue;
                if (++i == args.Length) throw new ArgumentException("Missing value for " + flag);
                if (flag == "-moyvaTrainingTimeScale")
                    config.visualTimeScale = config.headlessTimeScale = float.Parse(args[i], CultureInfo.InvariantCulture);
                else
                {
                    int value = int.Parse(args[i], CultureInfo.InvariantCulture);
                    if (flag == "-moyvaSeed") config.baseSeed = value;
                    else if (flag == "-moyvaWorldSize") config.worldSize = value;
                    else if (flag == "-moyvaEpisodeDecisions") config.maxDecisionsPerEpisode = value;
                    else config.curriculum.stage = (TrainingCurriculumStage)value;
                }
            }
            config.Validate();
        }
    }
}
