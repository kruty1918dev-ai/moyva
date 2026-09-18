from .config import ControlError, atomic_json, read_json, simple_name


class Presets:
    def __init__(self, project):
        self.project = project

    def defaults(self):
        config = self.project.training_config()
        max_episode_decisions = config.get("maxDecisionsPerEpisode", 1000)
        result = {}
        for stage, value in self.project.stages().items():
            name = {"BasicLifecycle": "smoke", "FullGame": "fullgame", "FogOfWar": "fog-of-war"}.get(stage, stage.lower())
            result[name] = dict(
                name=name,
                description="Real gameplay: " + stage,
                stage=value,
                max_steps=500000,
                episode_decisions=max_episode_decisions,
                seed=config.get("baseSeed", 1918),
                world_size=config.get("worldSize", 24),
                time_scale=config.get("headlessTimeScale", 10),
                visual=False,
                arenas=1,
                initialize_from="",
                learn_initial_castle=False,
                trainer=self.project.settings.get("trainer", "Assets/Moyva/AI/Training/Config/moyva_ppo.yaml"),
                checkpoint_interval=50000,
                summary_freq=1000,
                screen_width=1280,
                screen_height=720,
                results=str(self.project.results),
            )

        # Very short lifecycle/communicator check.
        result["smoke"].update(
            description="Pipeline smoke: BasicLifecycle",
            max_steps=512,
            checkpoint_interval=512,
            episode_decisions=min(64, max_episode_decisions),
            summary_freq=128,
        )

        # FullGame smoke must be long enough to execute real episodes. The previous
        # 128 steps / 30 decisions configuration mainly tested startup and could
        # terminate episodes before meaningful FullGame behavior occurred.
        result["fullgame-smoke"] = {
            **result["fullgame"],
            "name": "fullgame-smoke",
            "description": "FullGame smoke: real gameplay, short validation run",
            "max_steps": 10000,
            "checkpoint_interval": 10000,
            "episode_decisions": max_episode_decisions,
            "summary_freq": 500,
        }
        result["fullgame-preview"] = {
            **result["fullgame-smoke"],
            "name": "fullgame-preview",
            "description": "FullGame live preview: visible Unity training window",
            "visual": True,
            "time_scale": 1,
            "summary_freq": 250,
            "screen_width": 1280,
            "screen_height": 720,
        }
        result["long"] = {
            **result["fullgame"],
            "name": "long",
            "description": "Long FullGame training",
            "max_steps": 5000000,
            "summary_freq": 5000,
        }
        result["visual-debug"] = {
            **result["movement"],
            "name": "visual-debug",
            "description": "Visual movement/debug run",
            "visual": True,
            "time_scale": 1,
            "max_steps": 2048,
            "summary_freq": 256,
            "screen_width": 1280,
            "screen_height": 720,
        }
        result["castle-first"] = {
            **result["building"], "name": "castle-first", "visual": True, "time_scale": 1,
            "learn_initial_castle": True,
            "description": "Build your first castle through real construction; opponent starts with a settlement.",
        }
        result["arenas-preview"] = {
            **result["fullgame-preview"], "name": "arenas-preview", "arenas": 4,
            "description": "Four independent Unity arenas train one shared policy; one window per arena.",
        }
        return result

    def list(self):
        return {**self.defaults(), **read_json(self.project.local / "presets.json", {})}

    def get(self, name):
        try:
            return dict(self.list()[name])
        except KeyError:
            raise ControlError("Unknown preset: " + name)

    def validate(self, preset):
        arenas = preset.get("arenas", 1)
        if isinstance(arenas, bool) or int(arenas) != arenas or not 1 <= arenas <= 16:
            raise ControlError("arenas must be an integer between 1 and 16.")
        if preset.get("learn_initial_castle") and int(preset["stage"]) < 5:
            raise ControlError("Initial castle lesson requires Building or later curriculum.")
        if preset.get("initialize_from"): simple_name(preset["initialize_from"])
        if int(preset.get("base_port", 5005)) + arenas - 1 > 65535:
            raise ControlError("Arena ports exceed 65535.")
        if int(preset["stage"]) not in self.project.stages().values():
            raise ControlError("Unknown production curriculum stage.")
        for key, low, high in (
            ("world_size", 12, 128),
            ("time_scale", 0.1, 20),
            ("max_steps", 1, 10**12),
            ("episode_decisions", 1, 10**8),
            ("checkpoint_interval", 1, 10**12),
            ("summary_freq", 1, 10**9),
            ("screen_width", 640, 7680),
            ("screen_height", 360, 4320),
        ):
            if not low <= float(preset[key]) <= high:
                raise ControlError(f"{key} must be between {low} and {high}.")
        if not isinstance(preset.get("visual"), bool):
            raise ControlError("visual must be a boolean.")
        return preset

    def save(self, name, preset):
        simple_name(name)
        self.validate(preset)
        path = self.project.local / "presets.json"
        data = read_json(path, {})
        data[name] = {**preset, "name": name}
        atomic_json(path, data)
        return data[name]

    def reset(self, name):
        path = self.project.local / "presets.json"
        data = read_json(path, {})
        data.pop(name, None)
        atomic_json(path, data)
        return self.list().get(name)

    def clone_run(self, run_id, name):
        from .runs import RunStore

        run = RunStore(self.project).show(run_id, False)
        data = self.get("smoke")
        for key in ("stage", "seed", "world_size"):
            data[key] = run.get(key, data[key])
        data.update(
            visual=run.get("mode") == "Visual",
            trainer=str(RunStore(self.project).path(run_id) / "trainer.yaml"),
        )
        return self.save(name, data)
