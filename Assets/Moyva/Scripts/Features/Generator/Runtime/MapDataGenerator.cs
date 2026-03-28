// Features/Generator/Runtime/MapDataGenerator.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal class MapDataGenerator : IMapDataGenerator
    {
        private readonly INoiseProvider _noiseProvider;
        private readonly IBiomeResolver _biomeResolver;
        private readonly IEnumerable<IMapFeatureGenerator> _featureGenerators;
        private readonly DataNoiseSettings _noiseSettings;

        public MapDataGenerator(
            INoiseProvider noiseProvider,
            IBiomeResolver biomeResolver,
            IEnumerable<IMapFeatureGenerator> featureGenerators, 
            DataNoiseSettings noiseSettings)
        {
            _noiseProvider = noiseProvider;
            _biomeResolver = biomeResolver;
            _featureGenerators = featureGenerators;
            _noiseSettings = noiseSettings;
        }

        public async Task<string[,]> GenerateMapDataAsync(int width, int height)
        {
            // Використовуємо Task.Run, щоб важкі математичні обчислення 
            // не заморозили головний потік Unity (гра не зависне під час завантаження).
            return await Task.Run(() =>
            {
                float[,] heightMap = _noiseProvider.GenerateNoiseMap(
                    _noiseSettings, width, height);

                // 2. Визначаємо базові біоми (вода, пісок, трава, гори)
                string[,] virtualMap = _biomeResolver.ResolveBiomes(heightMap, width, height);

                // 3. Накладаємо додаткові фічі (річки, озера, ліси)
                // Порядок біндингу в Zenject визначить порядок їх виконання
                foreach (var featureGen in _featureGenerators)
                {
                    featureGen.ApplyFeatures(virtualMap, heightMap, width, height);
                }

                return virtualMap;
            });
        }
    }
}