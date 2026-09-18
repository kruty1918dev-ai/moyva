using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Marketing.Contracts
{
    [Serializable]
    public sealed class ShotRecord
    {
        public string shotId = string.Empty;
        public string category = string.Empty;
        public string rig = string.Empty;
        public string beat = string.Empty;
        public string subjectId = string.Empty;
        public int seed;
        public float durationSec;
        public float score;
        public string status = "pending";   // pending | ok | failed | skipped
        public string failure = string.Empty;
        public List<string> files = new List<string>();
    }

    [Serializable]
    public sealed class MarketingManifest
    {
        public string schema = "moyva.marketing-manifest";
        public int version = 1;
        public string runId = string.Empty;
        public string timestampUtc = string.Empty;
        public string status = "running";   // running | ok | partial | failed | cancelled
        public string recipeId = string.Empty;
        public string platformProfileId = string.Empty;
        public int resolutionWidth;
        public int resolutionHeight;
        public int frameRate;
        public float durationSec;
        public int worldSeed;
        public int scenarioSeed;
        public int cinematicSeed;
        public string sourceGameProcessSha = string.Empty;
        public string toolBranchSha = string.Empty;
        public string language = "en";
        public List<string> assetIds = new List<string>();
        public List<string> audioKeys = new List<string>();
        public List<string> filesGenerated = new List<string>();
        public List<string> validationIssues = new List<string>();
        public List<ShotRecord> shots = new List<ShotRecord>();
    }
}
