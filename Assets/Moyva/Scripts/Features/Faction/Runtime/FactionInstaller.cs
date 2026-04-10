using System.Collections.Generic;
using Kruty1918.Moyva.Faction.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Faction.Runtime
{
    /// <summary>
    /// Zenject MonoInstaller для модуля фракцій.
    /// Читає GameSessionConfigSO, будує FactionRegistry і реєструє всі сервіси.
    ///
    /// Підключіть цей installer у сцені разом з SignalBusInstaller.
    /// </summary>
    public sealed class FactionInstaller : MonoInstaller
    {
        [SerializeField] private GameSessionConfigSO _sessionConfig;

        public override void InstallBindings()
        {
            if (_sessionConfig == null)
            {
                Debug.LogError("[FactionInstaller] GameSessionConfigSO не призначено. Фракції не будуть зареєстровані.");
                return;
            }

            var definitions = BuildDefinitions(_sessionConfig);

            Container.BindInstance(new FactionRegistry(definitions))
                .AsSingle()
                .IfNotBound();

            Container.Bind<IFactionRegistry>()
                .FromResolve<FactionRegistry>()
                .AsSingle()
                .IfNotBound();

            Container.BindInterfacesAndSelfTo<FactionOwnershipService>()
                .AsSingle()
                .NonLazy();
        }

        private static List<FactionDefinition> BuildDefinitions(GameSessionConfigSO config)
        {
            var result = new List<FactionDefinition>(config.Factions.Count);
            foreach (var slot in config.Factions)
            {
                if (string.IsNullOrWhiteSpace(slot.FactionId))
                {
                    Debug.LogWarning("[FactionInstaller] Слот з порожнім FactionId пропущено.");
                    continue;
                }

                result.Add(new FactionDefinition(
                    new FactionId(slot.FactionId),
                    slot.Type,
                    slot.DefaultUnitTypeId,
                    slot.StartPosition,
                    slot.TeamColor));
            }
            return result;
        }
    }
}
