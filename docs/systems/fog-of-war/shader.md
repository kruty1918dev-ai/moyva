# Fog of War — Шейдер

← [README](README.md)

---

## Розташування

`Assets/Moyva/Scripts/Features/FogOfWar/Runtime/Shaders/FogOfWar.shader`

Назва шейдера: `Moyva/FogOfWar`

---

## URP 2D налаштування

```hlsl
Tags
{
    "RenderType"     = "Transparent"
    "Queue"          = "Transparent+100"
    "RenderPipeline" = "UniversalPipeline"
}
Pass { Tags { "LightMode" = "Universal2D" } }

Blend SrcAlpha OneMinusSrcAlpha
ZWrite Off
Cull Off
```

---

## Properties

| Property | Тип | Призначення |
|---|---|---|
| `_FogTex` | Texture2D | R8-текстура сітки туману |
| `_UnexploredColor` | Color | Колір непізнаної зони |
| `_ExploredColor` | Color | Колір пізнаної зони |
| `_NoiseScaleA` | Float | Масштаб шуму для Unexplored |
| `_NoiseSpeedA` | Float | Швидкість руху шуму для Unexplored |
| `_NoiseStrengthA` | Float | Сила впливу шуму на alpha для Unexplored |
| `_NoiseScaleB` | Float | Масштаб шуму для Explored |
| `_NoiseSpeedB` | Float | Швидкість руху шуму для Explored |
| `_NoiseStrengthB` | Float | Сила впливу шуму на alpha для Explored |
| `_EdgeBleedRadius` | Float | Радіус edge bleeding в тайлах |
| `_EdgeBleedStrength` | Float | Сила edge bleeding |
| `_TransitionSoftness` | Float | М'якість переходу між зонами |

---

## Три зони через smoothstep

R8 значення: 0 = Unexplored, ~0.502 = Explored, 1.0 = Visible

```hlsl
float wUnexplored = 1.0 - smoothstep(exploredLo, exploredHi, fogVal);
float wVisible    = smoothstep(visibleLo, 1.0, fogVal);
float wExplored   = saturate(1.0 - wUnexplored - wVisible);
```

---

## Perlin fBm (2 октави)

Вбудований шейдерний Perlin noise без зовнішніх текстур:

```hlsl
float fBm2(float2 p) {
    float v = 0; float a = 0.5; float2 s = 1;
    for (int i = 0; i < 2; i++) { v += a * GradNoise(p*s); s*=2; a*=0.5; }
    return v * 0.5 + 0.5;
}
```

---

## Edge Bleeding

Семплює 4 сусідні пікселі текстури. Якщо сусід темніший — fog "заходить" на тайл:

```hlsl
float minNeighbour = min(min(n0,n1), min(n2,n3));
float bleed = saturate((fogVal - minNeighbour) * _EdgeBleedStrength);
fogVal = fogVal - bleed;
```

---

## FallBack

```
FallBack "Universal Render Pipeline/2D/Sprite-Unlit-Default"
```
