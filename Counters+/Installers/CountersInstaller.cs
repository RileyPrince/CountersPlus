using CountersPlus.ConfigModels;
using CountersPlus.Counters;
using CountersPlus.Counters.Event_Broadcasters;
using CountersPlus.Counters.Interfaces;
using CountersPlus.Counters.NoteCountProcessors;
using CountersPlus.Custom;
using CountersPlus.Utils;
using System;
using UnityEngine;
using Zenject;

namespace CountersPlus.Installers
{
    class CountersInstaller : MonoInstaller
    {
        [Inject]
        private readonly HUDConfigModel hudConfig;

        [Inject]
        private readonly PlayerDataModel dataModel;

        public override void InstallBindings()
        {
            MainConfigModel mainConfig = Plugin.MainConfig;

            if (!mainConfig.Enabled) return;

            /// LOADING IMPORTANT SHIT LIKE CANVASES AND STUFF ///
            Container.Bind<CanvasUtility>().AsSingle();

            Container.Bind<NoteCountProcessor>().To<GenericNoteCountProcessor>().AsSingle();

            /// LOADING COUNTERS ///
            Plugin.Logger.Notice("Loading counters...");

            AddCounter<MissedConfigModel, MissedCounter>();
            AddCounter<NoteConfigModel, NotesCounter>();

            if (mainConfig.ProgressConfig.Mode != ProgressMode.BaseGame)
            {
                AddCounter<ProgressConfigModel, ProgressCounter>();
            }
            else
            {
                AddCounter<ProgressConfigModel, ProgressBaseGameCounter>();
            }

            AddCounter<ScoreConfigModel, ScoreCounter>();
            AddCounter<CutConfigModel, CutCounter>();
            AddCounter<FailConfigModel, FailCounter>();
            AddCounter<NotesLeftConfigModel, NotesLeftCounter>();
            AddCounter<SpeedConfigModel, SpeedCounter>();
            AddCounter<SpinometerConfigModel, Spinometer>();

            AddCounter<PBConfigModel, PBCounter>((settings) => {
                ScoreConfigModel scoreConfig = Container.Resolve<ScoreConfigModel>();
                HUDCanvas canvasSettings = GrabCanvasForCounter(scoreConfig);
                return scoreConfig.Enabled && settings.UnderScore && (dataModel.playerData.playerSpecificSettings.noTextsAndHuds ? canvasSettings.IgnoreNoTextAndHUDOption : true);
            });

            InstallCustomCounters();

            if (mainConfig.AprilFoolsTomfoolery && mainConfig.IsAprilFools)
            {
                Container.BindInterfacesAndSelfTo<AprilFools>().AsSingle().NonLazy();
            }

            /// LOADING BROADCASTERS WITH BROADCAST IN-GAME EVENTS TO COUNTERS AND STUFF ///
            Container.BindInterfacesAndSelfTo<CounterEventBroadcaster>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<NoteEventBroadcaster>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ScoreEventBroadcaster>().AsSingle().NonLazy();
        }

        protected virtual void InstallCustomCounters()
        {
            foreach (CustomCounter customCounter in Plugin.LoadedCustomCounters)
            {
                AddCustomCounter(customCounter, customCounter.CounterType);
            }
        }

        protected void AddCounter<T, R>() where T : ConfigModel where R : ICounter
        {
            AddCounter<T, R>(_ => false);
        }

        protected void AddCounter<T, R>(Func<T, bool> additionalReasonToSpawn) where T : ConfigModel where R : ICounter
        {
            T settings = Container.Resolve<T>();

            HUDCanvas canvasSettings = GrabCanvasForCounter(settings);

            if (!settings.Enabled || (!canvasSettings.IgnoreNoTextAndHUDOption && dataModel.playerData.playerSpecificSettings.noTextsAndHuds
                && !additionalReasonToSpawn(settings))) return;

            Plugin.Logger.Debug($"Loading counter {settings.DisplayName}...");

            if (typeof(MonoBehaviour).IsAssignableFrom(typeof(R)))
            {
                Container.BindInterfacesAndSelfTo<R>().FromNewComponentOnRoot().AsSingle().NonLazy();
            }
            else
            {
                Container.BindInterfacesAndSelfTo<R>().AsSingle().NonLazy();
            }
        }

        protected void AddCustomCounter(CustomCounter customCounter, Type counterType)
        {
            ConfigModel settings;

            if ((settings = Container.TryResolveId<ConfigModel>(customCounter.Name)) != null)
            {
                HUDCanvas canvasSettings = GrabCanvasForCounter(settings);

                if (!settings.Enabled || (!canvasSettings.IgnoreNoTextAndHUDOption && dataModel.playerData.playerSpecificSettings.noTextsAndHuds)) return;

                Plugin.Logger.Debug($"Loading counter {customCounter.Name}...");

                if (typeof(MonoBehaviour).IsAssignableFrom(counterType))
                {
                    Container.BindInterfacesAndSelfTo(counterType).FromNewComponentOnRoot().AsSingle().NonLazy();
                }
                else
                {
                    Container.BindInterfacesAndSelfTo(counterType).AsSingle().NonLazy();
                }
            }
        }

        protected HUDCanvas GrabCanvasForCounter(ConfigModel settings)
            => settings.CanvasID == -1 || settings.CanvasID >= hudConfig.OtherCanvasSettings.Count
            ? hudConfig.MainCanvasSettings
            : hudConfig.OtherCanvasSettings[settings.CanvasID];
    }
}
