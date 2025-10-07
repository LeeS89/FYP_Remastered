# FYP_Remastered
 
# 🧠 VR Stealth Remaster – Developer Growth Showcase

This is a remastered version of my final year Unity VR project. It is being rebuilt from the ground up to demonstrate my growth in architecture, performance, and system design. The original project was functional but featured tightly coupled systems, 
redundant calculations, and limited flexibility. This remaster focuses on an event-driven, performance-aware architecture.

---

## 🎯 Goals of the Remaster

- ✅ Replace tightly coupled systems with **event-driven architecture**
- ✅ Minimize use of `MonoBehaviour` via **pure C# classes**
- ✅ Reduce overhead from Update loops and coroutines
- ✅ Use **jobs and batching** for heavy operations
- ✅ Introduce a modular **FSM-based AI system**
- ✅ Implement **flanking AI behavior**
- ✅ Use **ScriptableObjects** to precompute and store costly data
- ✅ Create centralized logic for debugging and scalability

---

## 🧩 Architecture Overview

### Scene Loading & Setup
- **GameManager** finds the SceneManager and calls `SetUpScene()`
- SceneManager:
  - Loads assets (projectiles, particles, audio)
  - Creates object pools
  - Loops through all **EventManagers** and calls `BindComponentsToEvents()`

### Event System
- **Abstract `EventManager`** exists on every agent (player/enemy)
- Each child component uses interfaces to:
  - `RegisterLocalEvents()` – binds internal events (e.g., movement, collision)
  - `RegisterGlobalEvents()` – binds to global systems (e.g., PlayerDied, SceneStarted)
- Promotes **hierarchical setup** and modularity

---

## 🧠 AI System (FSM)

### Overview
- Each agent uses a `FSMController` with pure C# states.
- States include: **Patrol**, **Stationary**, **Chase**, **Flank**, **Death**

### FSM → Destination Flow
1. FSM state coroutine starts (e.g., Patrol)
2. Sends event to request a `DestinationType` (Patrol, Flank, etc.)
3. FSM handles the result, applies destination, and transitions animation/speed

### Flanking Logic
- Scene Editor places cube markers on NavMesh
- Each point stores:
  - Its `Vector3` position
  - A Dictionary: `<stepsAway, List<reachablePointIndexes>>`
- Stored in **ScriptableObject** to allow precomputation
- When flanking, agents query reachable points based on distance from player
- Flanking points are based around the nearest point to the player, based on distance from that point, other points are either 1 step, 2 steps up tp max steps away from the nearest point to the player
- Flanking is triggered when an agents view of the player becomes obstructed, Destination manager queries flanking points random steps from the player and returns the 1st reachable point 

---

## 🔧 Performance Improvements

| Feature | Original Project | Remastered |
|--------|------------------|------------|
| AI Logic | Tightly coupled `MonoBehaviours` | Event-driven FSM system |
| Pathfinding | Frequent per-agent `NavMesh.CalculatePath()` | Centralized **PathRequestManager** queue |
| Data Reuse | Runtime-heavy recalculations | **ScriptableObjects** for precomputed nav data |
| Object Pooling | None | Reusable pooled assets for bullets, particles |
| Draw Calls | Unbatched | **Dynamic batching + profiler-informed optimization** |
| Player Input | Hardcoded gesture logic | Decoupled gesture system via events |

---

## 💥 Bullet System

- Bullet is composed of:
  - **Movement component**
  - **Collision component**
  - **VFX component**
- Each component uses events to notify the others
- VFX pulls pooled particle effects for impact
- Bullets are reused via pooling for performance

---

## 🧍 Player Logic

- Gesture-based locomotion
- Gesture recognition events trigger movement logic
- Shares **stats system** with enemies (modular)

---

## 💡 Lessons Learned

- Stronger grasp on **abstraction and architecture**
- The power of **ScriptableObjects for static data**
- How to **profile and identify bottlenecks**
- Event systems and **decoupling for maintainability**
- How to structure for **scalability and testability**
- The value of **precomputed navigation data**

---

## 📸 Media (Coming Soon)

<!--
Drop in comparison gifs, before/after screenshots or diagrams here.
-->

---

## 🔗 Original Project

- [View Original College Submission](<img width="736" height="186" alt="image" src="https://github.com/user-attachments/assets/13c2f394-9438-4fd9-973b-05ba89040d5b" />
) 

---

## 🧪 Status

This remaster is in **early development**, with systems currently focused on core architecture, agent AI, and scene setup. Visuals and polish will come later.

