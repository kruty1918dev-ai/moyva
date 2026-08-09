using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor.Presets
{
    public static class BuildingPresetBatchService
    {
        public static BuildingPresetBatchResult ApplyAll(string source="manual") => Execute(validateOnly:false, source:source);
        public static BuildingPresetBatchResult ValidatePack(string source="manual") => Execute(validateOnly:true, source:source);

        public static BuildingPresetBatchResult ApplyPresetToSelected(string presetId, BuildingDefinitionAsset selected, bool preserveId=true)
        {
            if (selected == null) throw new InvalidOperationException("Select a BuildingDefinitionAsset first.");
            BuildingPresetPackSpec pack=BuildingJsonPresetSerializer.LoadPack(); BuildingPresetSpec spec=BuildingJsonPresetSerializer.LoadPreset(presetId); BuildingPresetValidation.ValidateSpec(spec); ResolvedBuildingPreset resolved=BuildingJsonPresetResolver.Resolve(spec,pack);
            BuildingDefinitionAsset stage=UnityEngine.Object.Instantiate(selected); stage.hideFlags=HideFlags.HideAndDontSave;
            try
            {
                stage.name = selected.name;
                BuildingJsonPresetApplier.ApplyToAsset(resolved,stage,preserveId); BuildingPresetValidation.ValidateRuntimeAsset(stage);
                string before=EditorJsonUtility.ToJson(selected,false); string after=EditorJsonUtility.ToJson(stage,false);
                int changed=string.Equals(before,after,StringComparison.Ordinal)?0:1;
                if (changed>0) { Undo.RecordObject(selected,"Apply building JSON preset"); EditorUtility.CopySerialized(stage,selected); selected.NotifyEditorDataChanged(); EditorUtility.SetDirty(selected); AssetDatabase.SaveAssetIfDirty(selected); }
                return new BuildingPresetBatchResult{PackId=pack.PackId,PresetCount=1,ChangedCount=changed,ValidateOnly=false,Summary=$"preset={presetId}, changed={changed}"};
            }
            finally { UnityEngine.Object.DestroyImmediate(stage); }
        }

        private static BuildingPresetBatchResult Execute(bool validateOnly,string source)
        {
            BuildingPresetPackSpec pack=BuildingJsonPresetSerializer.LoadPack();
            if (!string.Equals(pack.PackId,BuildingPresetPaths.ExpectedPackId,StringComparison.Ordinal)) throw new InvalidOperationException($"Unexpected preset pack '{pack.PackId}'.");
            if (pack.RequiresSchemaVersion!=1) throw new InvalidOperationException($"Unsupported required schema {pack.RequiresSchemaVersion}.");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            EnsureFolder(BuildingPresetPaths.DefinitionRoot);
            var resolved=new List<ResolvedBuildingPreset>();
            var ids=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach(string id in pack.PresetIds)
            {
                if(!ids.Add(id)) throw new InvalidOperationException($"Duplicate preset id in pack: {id}");
                BuildingPresetSpec spec=BuildingJsonPresetSerializer.LoadPreset(id); BuildingPresetValidation.ValidateSpec(spec); resolved.Add(BuildingJsonPresetResolver.Resolve(spec,pack));
            }
            Dictionary<string,BuildingDefinitionAsset> existing=FindExistingById(pack.PresetIds);
            var stages=new List<(ResolvedBuildingPreset resolved,BuildingDefinitionAsset stage,BuildingDefinitionAsset existing)>();
            try
            {
                foreach(ResolvedBuildingPreset item in resolved)
                {
                    existing.TryGetValue(item.Spec.Id,out BuildingDefinitionAsset current);
                    BuildingDefinitionAsset stage=current!=null?UnityEngine.Object.Instantiate(current):ScriptableObject.CreateInstance<BuildingDefinitionAsset>();
                    stage.name=item.Spec.Id; stage.hideFlags=HideFlags.HideAndDontSave; BuildingJsonPresetApplier.ApplyToAsset(item,stage,false); BuildingPresetValidation.ValidateRuntimeAsset(stage); stages.Add((item,stage,current));
                }
                BuildingRegistrySO registry=FindRegistry();
                if(validateOnly) return new BuildingPresetBatchResult{PackId=pack.PackId,PresetCount=stages.Count,ChangedCount=0,ValidateOnly=true,Summary=$"validated {stages.Count}/{stages.Count}"};
                int changed=0; var committed=new List<BuildingDefinitionAsset>();
                foreach(var tuple in stages)
                {
                    BuildingDefinitionAsset target=tuple.existing;
                    if(target==null)
                    {
                        target=ScriptableObject.CreateInstance<BuildingDefinitionAsset>(); string path=$"{BuildingPresetPaths.DefinitionRoot}/{tuple.resolved.Spec.Id}.asset";
                        if(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path)!=null) throw new InvalidOperationException($"Cannot create preset asset; path already occupied: {path}");
                        EditorUtility.CopySerialized(tuple.stage,target); target.name=tuple.resolved.Spec.Id; AssetDatabase.CreateAsset(target,path); changed++;
                    }
                    else
                    {
                        string before=EditorJsonUtility.ToJson(target,false); string after=EditorJsonUtility.ToJson(tuple.stage,false);
                        if(!string.Equals(before,after,StringComparison.Ordinal)) { EditorUtility.CopySerialized(tuple.stage,target); target.name=tuple.resolved.Spec.Id; target.NotifyEditorDataChanged(); EditorUtility.SetDirty(target); changed++; }
                    }
                    committed.Add(target);
                }
                NormalizeRegistry(registry,committed); AssetDatabase.SaveAssets();
                foreach(BuildingDefinitionAsset asset in committed) BuildingPresetValidation.ValidateRuntimeAsset(asset);
                var result=new BuildingPresetBatchResult{PackId=pack.PackId,PresetCount=committed.Count,ChangedCount=changed,ValidateOnly=false,Summary=$"source={source}, presets={committed.Count}, changed={changed}"};
                Debug.Log($"[MoyvaBuildingPresets] APPLY_OK pack={pack.PackId} changed={changed} source={source}"); return result;
            }
            catch(Exception ex)
            {
                Debug.LogError($"[MoyvaBuildingPresets] APPLY_ERROR pack={pack.PackId} source={source}: {ex}"); throw;
            }
            finally { foreach(var tuple in stages) if(tuple.stage!=null) UnityEngine.Object.DestroyImmediate(tuple.stage); }
        }

        private static Dictionary<string,BuildingDefinitionAsset> FindExistingById(IEnumerable<string> targetIds)
        {
            var targets=new HashSet<string>(targetIds,StringComparer.OrdinalIgnoreCase); var result=new Dictionary<string,BuildingDefinitionAsset>(StringComparer.OrdinalIgnoreCase); var duplicates=new List<string>();
            foreach(string guid in AssetDatabase.FindAssets("t:BuildingDefinitionAsset"))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid); BuildingDefinitionAsset asset=AssetDatabase.LoadAssetAtPath<BuildingDefinitionAsset>(path); if(asset==null||string.IsNullOrWhiteSpace(asset.Id)||!targets.Contains(asset.Id)) continue;
                if(result.TryGetValue(asset.Id,out BuildingDefinitionAsset prior) && prior!=asset) duplicates.Add($"{asset.Id}: {AssetDatabase.GetAssetPath(prior)} | {path}"); else result[asset.Id]=asset;
            }
            if(duplicates.Count>0) throw new InvalidOperationException("Duplicate target building IDs: "+string.Join(" || ",duplicates)); return result;
        }
        private static BuildingRegistrySO FindRegistry()
        {
            string[] guids=AssetDatabase.FindAssets("t:BuildingRegistrySO"); if(guids.Length==0) throw new InvalidOperationException("BuildingRegistrySO not found.");
            var candidates=new List<(BuildingRegistrySO registry,string path,int score)>();
            foreach(string guid in guids)
            {
                string path=AssetDatabase.GUIDToAssetPath(guid); BuildingRegistrySO registry=AssetDatabase.LoadAssetAtPath<BuildingRegistrySO>(path); if(registry==null) continue;
                int score=(registry.BuildingAssets?.Length??0)*10; if(path.EndsWith("/New Building Registry SO.asset",StringComparison.OrdinalIgnoreCase)) score+=10000; candidates.Add((registry,path,score));
            }
            if(candidates.Count==0) throw new InvalidOperationException("BuildingRegistrySO assets could not be loaded."); int best=candidates.Max(c=>c.score); var winners=candidates.Where(c=>c.score==best).ToList();
            if(winners.Count!=1) throw new InvalidOperationException("Ambiguous BuildingRegistrySO: "+string.Join(" | ",winners.Select(c=>c.path))); return winners[0].registry;
        }
        private static void NormalizeRegistry(BuildingRegistrySO registry,List<BuildingDefinitionAsset> committed)
        {
            var byId=new Dictionary<string,BuildingDefinitionAsset>(StringComparer.OrdinalIgnoreCase);
            foreach(BuildingDefinitionAsset asset in registry.BuildingAssets??Array.Empty<BuildingDefinitionAsset>()) if(asset!=null&&!string.IsNullOrWhiteSpace(asset.Id)) byId[asset.Id]=asset;
            foreach(BuildingDefinitionAsset asset in committed) byId[asset.Id]=asset;
            List<BuildingDefinitionAsset> ordered=byId.Values.OrderBy(a=>a.Id,StringComparer.OrdinalIgnoreCase).ToList();
            bool same=registry.BuildingAssets!=null && registry.BuildingAssets.Length==ordered.Count;
            if(same) for(int i=0;i<ordered.Count;i++) if(registry.BuildingAssets[i]!=ordered[i]) { same=false; break; }
            if(!same) { registry.SetBuildingAssets(ordered); EditorUtility.SetDirty(registry); }
        }
        private static void EnsureFolder(string path)
        {
            if(AssetDatabase.IsValidFolder(path)) return; string[] parts=path.Split('/'); string current=parts[0];
            for(int i=1;i<parts.Length;i++){string next=current+"/"+parts[i]; if(!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current,parts[i]); current=next;}
        }
    }
}
