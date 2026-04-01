# Fog of War — Texture Pipeline

← [README](README.md)

---

## Формат текстури

- `TextureFormat.R8` — 1 байт на піксель, лише R-канал
- Розмір: `mapWidth × mapHeight` пікселів
- `FilterMode.Bilinear` (за замовчуванням, налаштовується через `FogOfWarSettings.TextureFilter`)

---

## Значення пікселів

| FogStateType | byte (R8) | Hex |
|---|---|---|
| Unexplored | 0 | 0x00 |
| Explored | 128 | 0x80 |
| Visible | 255 | 0xFF |

---

## Dirty-Tiles оптимізація

`FogTextureUpdater` зберігає `byte[] _buffer` розміром `width * height`. При кожному оновленні:

1. Тільки змінені тайли (`dirtyTiles`) оновлюють `_buffer[y * width + x]`
2. Викликається `Texture2D.SetPixelData<byte>(_buffer, mipLevel: 0)`
3. Викликається `Texture2D.Apply(updateMipmaps: false, makeNoLongerReadable: false)`

Це дозволяє оновлювати навіть великі карти ефективно.

---

## Зв'язок із матеріалом

```csharp
_material.SetTexture("_FogTex", _fogTexture);
```

Виконується один раз при ініціалізації і після кожного `Apply`.

---

## Код SetPixelData

```csharp
_fogTexture.SetPixelData(_buffer, 0);
_fogTexture.Apply(false, false);
```

`Apply(false, false)` означає: не оновлювати mip-map, не забороняти читання з CPU. Це найшвидший шлях для часто оновлюваних текстур.

---

## Повна перебудова

`RebuildFullTexture(IFogOfWarService)` ітерує всі тайли і перемальовує повністю. Використовується при завантаженні гри зі snapshot.
