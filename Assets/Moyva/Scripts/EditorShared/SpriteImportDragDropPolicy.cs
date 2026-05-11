using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace Kruty1918.Moyva.Editor.Shared
{
    public static class SpriteImportDragDropPolicy
    {
        public static bool IsAllowedSprite(Sprite sprite, out string reason)
        {
            reason = string.Empty;
            if (sprite == null)
            {
                reason = "Sprite не призначений.";
                return false;
            }

            string path = AssetDatabase.GetAssetPath(sprite);
            if (string.IsNullOrWhiteSpace(path))
            {
                reason = "Sprite не є project asset.";
                return false;
            }

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                reason = $"'{path}' не імпортовано як текстура для спрайта.";
                return false;
            }

            if (importer.textureType != TextureImporterType.Sprite)
            {
                reason = $"'{path}' має Texture Type '{importer.textureType}', очікується Sprite.";
                return false;
            }

            if (importer.spriteImportMode == SpriteImportMode.None)
            {
                reason = $"'{path}' має Sprite Mode None.";
                return false;
            }

            return true;
        }

        public static bool EnsureAllowedSprite(ref Sprite sprite, string contextLabel)
        {
            if (sprite == null)
                return true;

            if (IsAllowedSprite(sprite, out string reason))
                return true;

            string invalidName = sprite != null ? sprite.name : "<null>";
            sprite = null;
            Debug.LogWarning($"[SpritePolicy] {contextLabel}: '{invalidName}' відхилено. {reason}");
            return false;
        }

        public static bool HandleDrop(Rect dropRect, ref Sprite target, string contextLabel)
        {
            Event evt = Event.current;
            if (evt == null)
                return false;

            if (!dropRect.Contains(evt.mousePosition))
                return false;

            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
                return false;

            if (TryResolveFirstAllowedSprite(DragAndDrop.objectReferences, out Sprite resolved, out string reason))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    target = resolved;
                    evt.Use();
                    return true;
                }

                evt.Use();
                return false;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            if (evt.type == EventType.DragPerform)
            {
                Debug.LogWarning($"[SpritePolicy] {contextLabel}: drop відхилено. {reason}");
                evt.Use();
            }
            else
            {
                evt.Use();
            }

            return false;
        }

        public static bool TryResolveFirstAllowedSprite(UnityEngine.Object[] droppedObjects, out Sprite sprite, out string reason)
        {
            sprite = null;
            reason = "Не знайдено допустимих sprite asset-ів.";
            if (droppedObjects == null || droppedObjects.Length == 0)
                return false;

            for (int i = 0; i < droppedObjects.Length; i++)
            {
                var obj = droppedObjects[i];
                if (TryResolveAllowedSpriteFromObject(obj, out sprite, out reason))
                    return true;
            }

            return false;
        }

        private static bool TryResolveAllowedSpriteFromObject(UnityEngine.Object obj, out Sprite sprite, out string reason)
        {
            sprite = null;
            reason = "Unsupported object.";
            if (obj == null)
                return false;

            if (obj is Sprite directSprite)
            {
                if (IsAllowedSprite(directSprite, out reason))
                {
                    sprite = directSprite;
                    return true;
                }

                return false;
            }

            if (obj is Texture2D texture)
            {
                string texturePath = AssetDatabase.GetAssetPath(texture);
                return TryResolveSpriteFromTexturePath(texturePath, out sprite, out reason);
            }

            if (obj is SpriteAtlas atlas)
            {
                var packables = SpriteAtlasExtensions.GetPackables(atlas);
                for (int i = 0; i < packables.Length; i++)
                {
                    var packable = packables[i];
                    if (packable is Sprite packableSprite && IsAllowedSprite(packableSprite, out reason))
                    {
                        sprite = packableSprite;
                        return true;
                    }

                    if (packable is Texture2D tex)
                    {
                        string path = AssetDatabase.GetAssetPath(tex);
                        if (TryResolveSpriteFromTexturePath(path, out sprite, out reason))
                            return true;
                    }

                    if (packable is DefaultAsset folder)
                    {
                        string folderPath = AssetDatabase.GetAssetPath(folder);
                        if (!string.IsNullOrWhiteSpace(folderPath) && AssetDatabase.IsValidFolder(folderPath))
                        {
                            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });
                            for (int g = 0; g < guids.Length; g++)
                            {
                                string spritePath = AssetDatabase.GUIDToAssetPath(guids[g]);
                                var found = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                                if (found != null && IsAllowedSprite(found, out reason))
                                {
                                    sprite = found;
                                    return true;
                                }
                            }
                        }
                    }
                }

                reason = "У SpriteAtlas не знайдено допустимих sprite-ів.";
                return false;
            }

            return false;
        }

        private static bool TryResolveSpriteFromTexturePath(string texturePath, out Sprite sprite, out string reason)
        {
            sprite = null;
            reason = "Texture path is empty.";
            if (string.IsNullOrWhiteSpace(texturePath))
                return false;

            var subAssets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
            var candidates = new List<Sprite>();
            for (int i = 0; i < subAssets.Length; i++)
            {
                if (subAssets[i] is Sprite s)
                    candidates.Add(s);
            }

            for (int i = 0; i < candidates.Count; i++)
            {
                if (IsAllowedSprite(candidates[i], out reason))
                {
                    sprite = candidates[i];
                    return true;
                }
            }

            reason = $"Texture '{texturePath}' не містить допустимих sprite-ів.";
            return false;
        }
    }
}
