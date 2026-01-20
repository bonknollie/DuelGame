# DuelGame: 1v1 Online Arena Combat

**20 January 2026**

---

## Table of Contents

| Section | Page |
|---------|------|
| INTRODUCTION | 3 |
| IMPLEMENTATION | 3 |
| CRITICAL REVIEW | 7 |
| CONCLUSION | 8 |
| ESTIMATED GRADE | 9 |
| REFERENCES | 10 |

---

## INTRODUCTION

DuelGame is a complete 1v1 player-versus-player duel system built in Unity with support for both local and online multiplayer. Players engage in real-time combat using melee attacks, defense/blocking mechanics, stamina management, and camera-relative movement in a confined arena. The system features server-authoritative hit detection to prevent exploits, smooth first-person camera controls with anti-spin attack limitations, and full animation synchronization via Unity's Netcode for GameObjects.

**Key Features:**
- Local and online 1v1 dueling
- Attack/defense/blocking with stamina costs and regeneration
- Smooth first-person mouse look with attack turn-cap to prevent spinning
- Melee weapon hit detection with segment-based sweeping
- Server-authoritative damage (prevents client-side cheating)
- Animation events for precise attack timing
- DebugStatsDisplay (health/stamina overlay)
- NetworkBootstrap UI for quick Host/Client launching

---

## IMPLEMENTATION

### Project Links

**Code Repository:**
- [GitHub Repository](https://github.com/bonknollie/DuelGame)

**Build/Playable:**
- [Itch.io / Build Link](https://itch.io) *(add link after building)*

**Video Demonstration:**
- [YouTube Video Link](https://youtube.com) *(record and add after online testing complete)*

### Setup Instructions

#### Offline Testing (No Netcode Install Required)
1. Add DuelSpawner to scene; assign player prefab and two spawn transforms
2. Press Play → two players spawn locally; second player is target dummy

#### Online Testing (Requires Netcode for GameObjects)
1. **Install Packages:**
   - Package Manager → `com.unity.netcode.gameobjects`
   - Package Manager → `com.unity.transport`

2. **Add Scripting Define:**
   - Project Settings → Player → Scripting Define Symbols → add `UNITY_NETCODE`

3. **Scene Setup:**
   - Create `NetworkManager` with `NetworkManager` + `UnityTransport` components
   - (Optional) Add `NetworkBootstrap` for Host/Client buttons
   - (Optional) Add `NetStatusHUD` for network status display

4. **Player Prefab:**
   - Add `NetworkObject` + `NetworkAnimator` to player root
   - Add `NetworkPlayerController` + `NetworkCombatAdapter`
   - Enable `Server Authority` on MeleeWeapon component

5. **Test:**
   - Build two instances
   - Instance 1: Click "Start Host"
   - Instance 2: Click "Start Client"
   - Fight and verify sync/damage

### Technical Architecture

#### Scripts
- **PlayerCombatController_Animated:** Attack/defense input, stamina, animation triggers, server validation hook
- **MeleeWeapon:** Sweep-based hit detection with segment sampling; server-authority gating
- **FirstPersonCamera:** Mouse look with SmoothDampAngle smoothing; attack turn-cap limits spinning
- **SimpleMovementController:** WASD relative to facing direction
- **NetworkPlayerController:** Owner-only camera/movement enablement
- **NetworkCombatAdapter:** Owner → Server RPC for attack validation
- **DuelSpawner:** Offline two-player spawner (no Netcode required)
- **NetStatusHUD:** On-screen network role/status display

#### Key Mechanics
- **Stamina System:** Attack costs 25, defense drains 10/sec, regenerates 15/sec
- **Blocking Mechanic:** Right-click to block; attacker is stunned if blocked
- **Animation Events:** SlashAttack fires OnAnimationAttackStart (~30%) and OnAnimationAttackEnd (~95%)
- **Server Authority:** Only server runs hit detection when Netcode is active; prevents client-side damage cheating
- **Anti-Exploit Turn Cap:** Yaw input capped to 120°/sec while attacking; prevents beyblade-style spinning

---

## CRITICAL REVIEW

**Strengths:**
- Complete end-to-end implementation from combat mechanics to network architecture
- Server-authoritative design prevents common exploitation vectors
- Smooth camera and movement controls with attack turn-cap limitation
- Offline fallback path allows testing without package dependencies
- Comprehensive animation integration with event-based attack timing

**Areas for Enhancement:**
- Latency compensation and client-side prediction could improve responsiveness
- Advanced networking features (matchmaking, lobby system, persistence)
- Extended balance tuning based on playtesting feedback
- Additional combat abilities and animation variety
- Network traffic optimization for larger player counts

**Testing Validation:**
- [ ] Local 1v1 duel (two-player offline)
- [ ] Online Host/Client connection
- [ ] Attack/Block mechanics with stamina gating
- [ ] Server-authority hit detection prevents client-side damage cheating
- [ ] Attack turn-cap limits spinning during attacks
- [ ] Animation synchronization across network
- [ ] Health/stamina UI overlay display

---

## CONCLUSION

DuelGame delivers a functional 1v1 online combat system suitable for real-time multiplayer dueling. The implementation balances gameplay accessibility with anti-exploit protections through server-authoritative design. Both offline (local two-player) and online (Netcode) paths are available, providing flexibility for testing and deployment. The foundation is complete and ready for balance iteration, content expansion, and deployment to players.

---

## ESTIMATED GRADE

*Grade assessment pending after video recording and online testing validation.*

---

## REFERENCES

- Unity Documentation: Netcode for GameObjects - https://docs.unity.com/netcode/
- Unity Documentation: Unity Transport - https://docs.unity.com/transport/
- Unity Input System Documentation - https://docs.unity3d.com/Packages/com.unity.inputsystem/
- GitHub Repository: [DuelGame Source](https://github.com/user/DuelGame)

---

**Document Version:** 1.0  
**Last Updated:** 20 January 2026
