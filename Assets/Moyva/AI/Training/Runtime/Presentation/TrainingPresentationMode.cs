using System;

namespace Kruty1918.Moyva.AI.Training
{
    public enum TrainingPresentationMode { Visual, MetricsOnly, HeadlessFast }

    public static class TrainingPresentationModeResolver
    {
        public static TrainingPresentationMode Resolve(TrainingConfig config, bool batchMode, string[] arguments)
        {
            var mode = config.presentationMode;
            for (int i = 0; i < arguments.Length; i++)
                if (arguments[i] == "-moyvaTrainingMode")
                {
                    if (++i >= arguments.Length || !Enum.TryParse(arguments[i], true, out mode)
                        || !Enum.IsDefined(typeof(TrainingPresentationMode), mode))
                        throw new ArgumentException("-moyvaTrainingMode requires Visual, MetricsOnly or HeadlessFast.");
                }
            return batchMode && config.autoHeadlessInBatchMode ? TrainingPresentationMode.HeadlessFast : mode;
        }
    }
}
