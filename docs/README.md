# 🎮 FUTA Rush — Complete Unity Project

> An endless runner mobile game set on the Federal University of Technology Akure (FUTA) campus.
> Inspired by Subway Surfers. Built for Android using Unity 6 LTS.

---

## 🏗️ Project Structure

```
FUTARush/
├── Assets/
│   └── _FUTARush/
│       ├── Scripts/
│       │   ├── Core/
│       │   │   ├── EventBus.cs           ← Decoupled pub/sub event system
│       │   │   ├── GameManager.cs        ← Central state machine + speed
│       │   │   ├── SceneLoader.cs        ← Fade-transition scene loading
│       │   │   └── WorldInitializer.cs   ← Bootstraps GamePlay scene
│       │   ├── Player/
│       │   │   ├── PlayerController.cs   ← Lane switch, jump, slide, death
│       │   │   ├── SwipeInput.cs         ← Touch swipe + Editor keys
│       │   │   ├── PlayerAnimator.cs     ← Animator bridge
│       │   │   └── CameraFollow.cs       ← Smooth camera trailing
│       │   ├── World/
│       │   │   ├── TileManager.cs        ← Procedural infinite tile spawning
│       │   │   ├── ObstacleSpawner.cs    ← Campus obstacles in random lanes
│       │   │   ├── CoinSpawner.cs        ← School token row spawning
│       │   │   ├── TokenRotate.cs        ← Token spin + bob + auto-despawn
│       │   │   └── ObstacleReturnToPool.cs ← Auto-recycle obstacles
│       │   ├── Systems/
│       │   │   └── ObjectPooler.cs       ← Generic object pool (zero alloc)
│       │   └── UI/
│       │       └── UIManager.cs          ← Home / HUD / Pause / Game Over
│       ├── Prefabs/
│       │   ├── Tiles/                    ← Road tile prefabs (see below)
│       │   ├── Obstacles/                ← Bus, barrier, guard prefabs
│       │   ├── Collectibles/             ← Token prefab
│       │   └── FX/                       ← Particle systems
│       ├── Scenes/
│       │   ├── HomeScreen.unity
│       │   └── GamePlay.unity
│       ├── Materials/
│       ├── Textures/
│       └── Audio/
├── Packages/
│   └── manifest.json
└── ProjectSettings/
```

---

## ⚙️ Step-by-Step Unity Setup

### 1. Create the Project

1. Open **Unity Hub**
2. Click **New Project**
3. Select template: **3D (URP)** — Unity 6 LTS (6000.x)
4. Name: `FUTARush`
5. Click **Create**

---

### 2. Import Scripts

Copy the entire `Assets/_FUTARush/Scripts/` folder into your Unity project.
Unity will compile all scripts automatically.

---

### 3. Install TextMeshPro

```
Window → Package Manager → TextMeshPro → Install
```
Then: `Window → TextMeshPro → Import TMP Essential Resources`

---

### 4. Create Scenes

#### HomeScreen.unity
1. `File → New Scene → Basic (URP)` → Save as `HomeScreen`
2. Add **Canvas** (Screen Space – Overlay, Scale With Screen Size 1080×1920)
3. Add to Canvas:
   - Background Image (purple→orange gradient, matching reference image)
   - FUTA Rush graffiti logo image
   - `Button` — "TAP TO PLAY" (orange, rounded corners)
   - 4 icon buttons (Facebook, Google, Apple, Settings)
4. Create empty GameObject `SceneLoader` → add `SceneLoader.cs`
   - Add full-screen black `Image` child called `FadePanel` → assign to script
5. Create empty GameObject `UIManager` → add `UIManager.cs` → wire `_playButton`

#### GamePlay.unity
1. `File → New Scene → Basic (URP)` → Save as `GamePlay`
2. Add all GameObjects listed in the table below

---

### 5. GamePlay Scene GameObjects

| GameObject        | Component(s)                                          | Notes                        |
|-------------------|-------------------------------------------------------|------------------------------|
| `GameManager`     | `GameManager.cs`                                      | Set speed values             |
| `ObjectPooler`    | `ObjectPooler.cs`                                     | Configure pools (see below)  |
| `TileManager`     | `TileManager.cs`                                      |                              |
| `ObstacleSpawner` | `ObstacleSpawner.cs`                                  |                              |
| `CoinSpawner`     | `CoinSpawner.cs`                                      |                              |
| `WorldInit`       | `WorldInitializer.cs`                                 | Assign all references        |
| `Player`          | `CharacterController` + `PlayerController.cs` + `SwipeInput.cs` + `PlayerAnimator.cs` | Tag: **Player** |
| `Main Camera`     | `CameraFollow.cs`                                     | Target: Player               |
| `UICanvas`        | `UIManager.cs`                                        | Wire all panels + buttons    |
| `SceneLoader`     | `SceneLoader.cs`                                      | Black FadePanel Image child  |

---

### 6. ObjectPooler Configuration

In Inspector, add these Pool entries:

| Tag                | Prefab               | Size |
|--------------------|----------------------|------|
| `TileStraight`     | TileStraight.prefab  | 8    |
| `TileCampus`       | TileCampus.prefab    | 6    |
| `ObstacleBarrier`  | ObstacleBarrier      | 10   |
| `ObstacleBus`      | ObstacleBus          | 6    |
| `ObstacleGuard`    | ObstacleGuard        | 6    |
| `Token`            | Token.prefab         | 80   |

---

### 7. Player Setup

1. Create Capsule (or import humanoid model)
2. Tag: `Player`, Layer: `Player`
3. Add: `CharacterController` (height: 1.8, radius: 0.4)
4. Add: `PlayerController.cs`, `SwipeInput.cs`, `PlayerAnimator.cs`
5. Create **Animator Controller** with states:
   - `Run` → looping, default state
   - `Jump` → one-shot → transitions back to Run
   - `Slide` → one-shot → transitions back to Run
   - `Dead` → one-shot, no exit
   *(States must be named exactly as above — CrossFadeInFixedTime uses name hashes)*

---

### 8. Tile Prefab Structure

Each tile should be:
- **20 units long × 9 units wide** (3 lanes × 3 units each)
- Z origin at tile start

```
TileStraight/
  ├── Road         (grey asphalt mesh, lane markings)
  ├── LeftSide     (palm trees, FUTA admin building)
  ├── RightSide    (library building, hedges)
  └── Kerbs        (red-white striped borders)
```

---

### 9. Obstacle Prefabs

Each obstacle:
- Tag: `Obstacle`
- Has `BoxCollider` (NOT trigger)
- Has `ObstacleReturnToPool.cs`

| Prefab Name        | Description                          |
|--------------------|--------------------------------------|
| `ObstacleBarrier`  | Red/white campus security barrier    |
| `ObstacleBus`      | FUTA Shuttle bus (blue/yellow/green) |
| `ObstacleGuard`    | Security officer with German Shepherd|

---

### 10. Token Prefab

1. Create coin mesh (flat cylinder) with ₦ naira symbol texture
2. Tag: `Token`
3. Add: `SphereCollider` → **Is Trigger: ✅**
4. Add: `TokenRotate.cs`
5. Gold material (`#FFD700`)

---

### 11. Add Scenes to Build

```
File → Build Settings → Scenes In Build
Add: Assets/_FUTARush/Scenes/HomeScreen.unity  (index 0)
Add: Assets/_FUTARush/Scenes/GamePlay.unity    (index 1)
```

---

## 📱 Android APK Build

### Player Settings
```
Edit → Project Settings → Player

Company Name:       FUTA Games
Product Name:       FUTA Rush
Version:            1.0.0
Bundle Identifier:  ng.edu.futa.futarush

Minimum API Level:  Android 6.0 (API 23)
Target API Level:   Android 14 (API 34)

Orientation:        Portrait (locked)
Graphics API:       Vulkan → OpenGLES3 (fallback)
Scripting Backend:  IL2CPP
Target Architecture: ARM64 ✅
```

### Build APK
```
File → Build Settings → Android → Switch Platform
Click: Build
→ Select output folder
→ FUTARush.apk is generated
```

### Install on Device
```bash
adb install FUTARush.apk
```
Or use **Build and Run** with USB-connected device.

---

## 🎮 How Procedural Generation Works

```
FRAME UPDATE LOOP:

  Player.Z increases each frame (forward movement)
  
  TileManager:
    while (lastTileZ < Player.Z + tilesAhead × tileLength)
      → Pick random tag from _tileTags[]
      → ObjectPooler.Spawn(tag, position)
      → Add to _activeTiles list
    
    for each activeTile:
      if tile.Z < Player.Z - tileLength
        → ObjectPooler.Despawn(tile)   // SetActive(false)
        → Remove from list

  ObstacleSpawner:
    while (nextSpawnZ < Player.Z + 30)
      → Random.value > spawnChance? skip
      → Random lane (0,1,2)
      → ObjectPooler.Spawn(randomObstacleTag, lane, nextSpawnZ)
      → nextSpawnZ += spawnInterval

  CoinSpawner:
    while (nextSpawnZ < Player.Z + 35)
      → Random lane
      → Spawn row of N tokens spaced 1.4 units apart
      → nextSpawnZ += rowInterval

RESULT: Infinite level, zero garbage collection at runtime
```

---

## 🎨 FUTA Color Palette

| Color            | Hex       | Usage                          |
|------------------|-----------|--------------------------------|
| Primary Orange   | `#FF6B00` | Buttons, rewards, UI accents   |
| FUTA Green       | `#1A7C3E` | Green shuttle bus, logo accent |
| Token Gold       | `#FFD700` | School tokens (₦ coins)        |
| Deep Purple      | `#3D1A6E` | Home screen gradient base      |
| Road Grey        | `#4A4A4A` | Asphalt road surface           |
| Kerb Red         | `#CC2200` | Campus kerb stripes            |
| Kerb White       | `#F5F5F5` | Campus kerb stripes            |
| Sky Blue         | `#87CEEB` | Background sky                 |

---

## 🌴 FUTA Campus Environment

### Background Props
- Senate Building (right side)
- Main Library (visible in gameplay reference)
- Admin / Office Building (left side)
- Palm trees (scattered throughout)
- Red-and-white striped kerbs
- Hedgerow bushes as lane dividers
- Railway track visible on left lane border

### Road Style
- 3-lane grey asphalt road
- White dashed centre lane markings
- Red/white kerbs on both edges (matches campus photo)

### Obstacle Characters
- FUTA Security Guard chasing player (with German Shepherd dog)
- FUTA Shuttle Buses: Blue, Yellow, Green variants
- Campus barriers: Red/white striped barricades

---

## ✅ Pre-Play Checklist

- [ ] All .cs files saved with matching class names
- [ ] `Player` tag set on Player GameObject
- [ ] `Obstacle` tag set on all obstacle prefabs
- [ ] `Token` tag set on Token prefab
- [ ] ObjectPooler pools fully configured
- [ ] WorldInitializer references all assigned
- [ ] UIManager all buttons + panels wired
- [ ] SceneLoader FadePanel Image assigned
- [ ] Both scenes in Build Settings (HomeScreen index 0, GamePlay index 1)
- [ ] TMP Essential Resources imported
- [ ] Android Build Module installed in Unity Hub

---

*FUTA Rush — Built with ❤️ for FUTA students*
