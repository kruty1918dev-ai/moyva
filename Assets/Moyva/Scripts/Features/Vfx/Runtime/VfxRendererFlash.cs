using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Vfx.Runtime
{
    /// <summary>
    /// Short MaterialPropertyBlock color flash on existing renderers (combat
    /// hit feedback). Snapshots the renderer's current block, writes a bright
    /// tint for ~0.1s, then restores the snapshot — safe alongside other MPB
    /// users because nothing is created or destroyed.
    /// </summary>
    public sealed class VfxRendererFlash
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int LegacyColorId = Shader.PropertyToID("_Color");

        private struct ActiveFlash
        {
            public Renderer Renderer;
            public MaterialPropertyBlock Previous;
            public float EndAt;
        }

        private readonly List<ActiveFlash> _active = new List<ActiveFlash>(16);
        private readonly Stack<MaterialPropertyBlock> _blocks =
            new Stack<MaterialPropertyBlock>(16);
        private readonly List<Renderer> _rendererBuffer = new List<Renderer>(16);
        private readonly Func<float> _clock;

        public Color FlashColor = new Color(2.6f, 1.1f, 0.9f, 1f);
        public float Duration = 0.12f;

        public VfxRendererFlash(Func<float> clock = null)
        {
            _clock = clock ?? (() => Time.time);
        }

        public int ActiveCount => _active.Count;

        public void Flash(GameObject target)
        {
            if (target == null)
                return;

            _rendererBuffer.Clear();
            target.GetComponentsInChildren(false, _rendererBuffer);
            if (_rendererBuffer.Count == 0)
                return;

            float endAt = _clock() + Duration;
            foreach (Renderer renderer in _rendererBuffer)
            {
                if (renderer == null)
                    continue;

                int propertyId = ResolveColorProperty(renderer);
                if (propertyId == 0)
                    continue;

                MaterialPropertyBlock previous = RentBlock();
                renderer.GetPropertyBlock(previous);

                MaterialPropertyBlock flash = RentBlock();
                renderer.GetPropertyBlock(flash);
                flash.SetColor(propertyId, FlashColor);
                renderer.SetPropertyBlock(flash);
                ReturnBlock(flash);

                _active.Add(new ActiveFlash
                {
                    Renderer = renderer,
                    Previous = previous,
                    EndAt = endAt,
                });
            }
        }

        public void Tick()
        {
            float now = _clock();
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                ActiveFlash flash = _active[i];
                if (flash.EndAt > now)
                    continue;

                if (flash.Renderer != null)
                    flash.Renderer.SetPropertyBlock(flash.Previous);

                ReturnBlock(flash.Previous);
                _active.RemoveAt(i);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < _active.Count; i++)
            {
                ActiveFlash flash = _active[i];
                if (flash.Renderer != null)
                    flash.Renderer.SetPropertyBlock(flash.Previous);
                ReturnBlock(flash.Previous);
            }
            _active.Clear();
        }

        private static int ResolveColorProperty(Renderer renderer)
        {
            Material material = renderer.sharedMaterial;
            if (material == null)
                return 0;
            if (material.HasProperty(BaseColorId))
                return BaseColorId;
            if (material.HasProperty(LegacyColorId))
                return LegacyColorId;
            return 0;
        }

        private MaterialPropertyBlock RentBlock()
            => _blocks.Count > 0 ? _blocks.Pop() : new MaterialPropertyBlock();

        private void ReturnBlock(MaterialPropertyBlock block)
        {
            block.Clear();
            _blocks.Push(block);
        }
    }
}
