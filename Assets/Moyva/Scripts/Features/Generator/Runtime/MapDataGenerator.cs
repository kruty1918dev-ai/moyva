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
        public IEnumerator GenerateMapDataRoutine(int width, int height, Action<string[,]> onComplete)
        {
            float[,] heightMap = _noiseProvider.GenerateNoiseMap(_noiseSettings, width, height);
            // Даємо Unity "дихнути" після генерації шуму
            yield return null;

            string[,] virtualMap = null;
            yield return _virtualHeightMapGenerator.GenerateVirtualHeightMapRoutine(heightMap, result => virtualMap = result);
            yield return null;

            if (_generationRules.GenerateBiomes)
            {
                yield return _biomeResolver.ResolveBiomesRoutine(heightMap, result => virtualMap = result);
                yield return null;
            }

            if (_generationRules.GenerateRivers)
            {
                foreach (var featureGen in _featureGenerators)
                {
                    // Тут ми можемо використовувати UnityEngine.Random!
                    yield return featureGen.ApplyFeaturesRoutine(virtualMap, heightMap, width, height);

                    // Якщо генерація фічі довга, можна "скидати" кадр всередині циклу
                    yield return null;
                }
            }

            if (_generationRules.ApplyWFC)
            {
                _wfcService.Apply(virtualMap, heightMap);
                yield return null;
            }

            onComplete?.Invoke(virtualMap);
        }
    }
}