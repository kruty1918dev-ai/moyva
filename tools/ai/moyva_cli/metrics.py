"""Bounded TensorBoard scalar reads, cached until event files change."""
from pathlib import Path
import time

class MetricReader:
    def __init__(self): self.cache = {}; self.accumulators = {}
    def read(self, directory):
        directory = Path(directory)
        files = list(directory.rglob("events.out.tfevents.*"))
        signature = tuple((str(p), p.stat().st_size, p.stat().st_mtime_ns) for p in files if p.is_file())
        old = self.cache.get(str(directory))
        if old and old[0] == signature: return old[1]
        result = {"series":{}, "step":None, "mean_reward":None, "episode_length":None, "steps_per_second":None}
        try:
            from tensorboard.backend.event_processing.event_accumulator import EventAccumulator
            for folder in sorted({p.parent for p in files}):
                accumulator = self.accumulators.get(str(folder))
                if accumulator is None:
                    accumulator = EventAccumulator(str(folder), size_guidance={"scalars":256})
                    if len(self.accumulators) >= 16: self.accumulators.pop(next(iter(self.accumulators)))
                    self.accumulators[str(folder)] = accumulator
                accumulator.Reload()
                for tag in accumulator.Tags().get("scalars", []):
                    points = [{"step":p.step, "value":p.value, "time":p.wall_time} for p in accumulator.Scalars(tag)]
                    if points: result["series"][tag] = points[-128:]
            for tag, points in result["series"].items():
                latest = points[-1]
                result["step"] = max(result["step"] or 0, latest["step"])
                if tag == "Environment/Cumulative Reward": result["mean_reward"] = latest["value"]
                if tag == "Environment/Episode Length": result["episode_length"] = latest["value"]
            for points in result["series"].values():
                if len(points) > 1 and points[-1]["time"] > points[-2]["time"]:
                    result["steps_per_second"] = (points[-1]["step"]-points[-2]["step"])/(points[-1]["time"]-points[-2]["time"])
                    break
        except ImportError: result["unavailable"] = "Install TensorBoard to read structured metrics."
        except Exception as error: result["unavailable"] = "Event stream is incomplete: " + str(error)
        if len(self.cache) >= 64 and str(directory) not in self.cache: self.cache.pop(next(iter(self.cache)))
        self.cache[str(directory)] = signature, result
        return result

METRICS = MetricReader()
