using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MapDataGenerator : IMapDataGenerator
    {
        private readonly INoiseProvider _noiseProvider;
        private readonly IVirtualHeightMapGenerator _virtualHeightMapGenerator;
        private readonly IBiomeResolver _biomeResolver;
        private readonly IEnumerable<IMapFeatureGenerator> _featureGenerators;
        private readonly IWFCService _wfcService;

        private readonly DataNoiseSettings _noiseSettings;
        private readonly GenerationRules _generationRules; // Твої правила

        public MapDataGenerator(
            INoiseProvider noiseProvider,
            IVirtualHeightMapGenerator virtualHeightMapGenerator,
            IBiomeResolver biomeResolver,
            IEnumerable<IMapFeatureGenerator> featureGenerators,
            DataNoiseSettings noiseSettings,
            GenerationRules generationRules,
            IWFCService wfcService)
        {
            _noiseProvider = noiseProvider;
            _virtualHeightMapGenerator = virtualHeightMapGenerator;
            _biomeResolver = biomeResolver;
            _featureGenerators = featureGenerators;
            _noiseSettings = noiseSettings;
            _generationRules = generationRules;
            _wfcService = wfcService;
        }

        // Інтерфейс тепер має повертати IEnumerator або використовувати Callbacks
        public void GenerateMapData(int width, int height, Action<string[,], string[,], float[,], string[,]> onComplete)
        {
            var previousRandomState = UnityEngine.Random.state;
            UnityEngine.Random.InitState(_noiseSettings.Seed);

            try
            {
            string[,] objectMap = new string[width, height];
            string[,] buildingMap = new string[width, height];
            float[,] heightMap = _noiseProvider.GenerateNoiseMap(_noiseSettings, width, height);
            
            string[,] virtualMap = null;
            _virtualHeightMapGenerator.GenerateVirtualHeightMap(heightMap, result => virtualMap = result);

            if (_generationRules.GenerateBiomes)
            {
                _biomeResolver.ResolveBiomes(heightMap, virtualMap, result =>
                {
                    // Тепер virtualMap не перезаписується повністю "пустим" результатом,
                    // а отримує оновлену версію себе
                    virtualMap = result;
                });
            }

            if (_generationRules.GenerateRivers)
            {
                foreach (var featureGen in _featureGenerators)
                {
                    // Runtime feature generators may use UnityEngine.Random;
                    // seeding above keeps runs deterministic.
                    featureGen.ApplyFeatures(virtualMap, objectMap, heightMap, width, height);

                }
            }

            if (_generationRules.ApplyWFC)
            {
                _wfcService.Apply(virtualMap, heightMap);
            }

            onComplete?.Invoke(virtualMap, objectMap, heightMap, buildingMap);
            }
            finally
            {
                UnityEngine.Random.state = previousRandomState;
            }
        }
    }
}