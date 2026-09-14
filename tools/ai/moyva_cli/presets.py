from .config import ControlError, atomic_json, read_json, simple_name

class Presets:
    def __init__(self, project): self.project = project
    def defaults(self):
        config = self.project.training_config()
        result = {}
        for stage, value in self.project.stages().items():
            name = {"BasicLifecycle":"smoke","FullGame":"fullgame","FogOfWar":"fog-of-war"}.get(stage, stage.lower())
            result[name] = dict(name=name, description="Real gameplay: " + stage, stage=value, max_steps=500000,
                episode_decisions=config.get("maxDecisionsPerEpisode",1000), seed=config.get("baseSeed",1918),
                world_size=config.get("worldSize",24), time_scale=config.get("headlessTimeScale",10), visual=False,
                trainer=self.project.settings.get("trainer","Assets/Moyva/AI/Training/Config/moyva_ppo.yaml"),
                checkpoint_interval=50000, results=str(self.project.results))
        result["smoke"].update(max_steps=128, checkpoint_interval=128, episode_decisions=20)
        for name, source, changes in [("fullgame-smoke","fullgame",dict(max_steps=128,checkpoint_interval=128,episode_decisions=30)),
                ("long","fullgame",dict(max_steps=5000000)),("visual-debug","movement",dict(visual=True,time_scale=1,max_steps=2048))]:
            result[name] = {**result[source], **changes, "name":name}
        return result
    def list(self): return {**self.defaults(), **read_json(self.project.local / "presets.json", {})}
    def get(self, name):
        try: return dict(self.list()[name])
        except KeyError: raise ControlError("Unknown preset: " + name)
    def validate(self, preset):
        if int(preset["stage"]) not in self.project.stages().values(): raise ControlError("Unknown production curriculum stage.")
        for key, low, high in (("world_size",12,128),("time_scale",0.1,20),("max_steps",1,10**12),
                              ("episode_decisions",1,10**8),("checkpoint_interval",1,10**12)):
            if not low <= float(preset[key]) <= high: raise ControlError(f"{key} must be between {low} and {high}.")
        if not isinstance(preset.get("visual"),bool): raise ControlError("visual must be a boolean.")
        return preset
    def save(self, name, preset):
        simple_name(name); self.validate(preset)
        path = self.project.local / "presets.json"
        data = read_json(path, {}); data[name] = {**preset,"name":name}; atomic_json(path,data)
        return data[name]
    def reset(self, name):
        path = self.project.local / "presets.json"; data = read_json(path,{})
        data.pop(name,None); atomic_json(path,data)
        return self.list().get(name)
    def clone_run(self, run_id, name):
        from .runs import RunStore
        run = RunStore(self.project).show(run_id, False)
        data = self.get("smoke")
        for key in ("stage","seed","world_size"): data[key] = run.get(key,data[key])
        data.update(visual=run.get("mode")=="Visual",trainer=str(RunStore(self.project).path(run_id)/"trainer.yaml"))
        return self.save(name,data)
