# Ex Umbris

**Genre:** Strategy / Simulation

**Perspective:** 2D, board-game-style map

**Core Fantasy:** Rise from obscurity to become a person of power and influence in a volatile frontier sector of space.

---

# 1. High-Level Concept

## Overview
The player begins as a relatively minor figure in a procedurally generated frontier sector — a ship captain, mayor, military officer, or other role — and must navigate a living world of autonomous NPCs, factions, and objectives. Power is gained not through conquest, but through influence, reputation, wealth, and favors.

The game emphasizes indirect control and emergent systems. The player shapes outcomes by creating objectives (bounties, contracts, missions) and watching autonomous NPCs respond according to their goals and personalities. Over time, this creates an intricate, reactive simulation where the player feels like they’re steering the currents of a living political and economic ecosystem.

## Design Pillars
- **Influence, not control:** The player shapes the world by incentives and reputation, not direct commands.
- **Living world simulation:** NPCs pursue their own goals and interact even without player input.
- **Transparency:** Game state is highly visible, with clear systems that reward understanding.
- **Emergent narrative:** Stories arise naturally from systemic interactions, not scripts.
- **Strategic pacing:** Interrupt-driven time simulation allows thoughtful planning and reaction.

---

# 2. Gameplay

## Core Loop
1. **Assess the world state:** Examine map, NPC objectives, faction standings, and personal resources.
2. **Set or pursue objectives:** Post bounties, take on contracts, or act directly.
3. **Advance time:** Let days pass, observing the simulation evolve until a trigger or limit occurs.
4. **React and adapt:** Adjust strategies, manage favors, and seize opportunities created by NPC actions.
5. **Progress:** Gain traits, wealth, reputation, and favors to unlock new strategic tools.

## Player Role
The player is an individual character within the world, not an omniscient faction controller. Over time, they can rise to prominence (commanding fleets, running corporations, or becoming a political powerbroker) but always as a participant in a larger world, not as its sole architect.

---

# 3. Systems

## 3.1 Time and Simulation
- **Interrupt-based time system:**
 - The world updates in "ticks" representing days.
 - The player sets an interval (e.g., max one week) or relies on triggers (events, messages, objectives) to pause time.
 - Simulation continues even while the player observes; NPCs act autonomously.

## 3.2 NPCs
- **Scale:** Hundreds of active NPCs, representing key figures (leaders, traders, mercenaries, smugglers, officers, etc.).
- **Autonomy:** Each NPC maintains internal goals and generates or fulfills objectives.
- **Persistence:** NPCs retain traits, relationships, and memories of interactions (favors, grudges, betrayals).
- **Traits System:**
 - Traits define personality and bonuses (e.g., "Ambitious," "Mercantile," "Vengeful").
 - Acquired traits modify behavior and stats.
- **Social Web:** NPCs form and dissolve alliances dynamically, creating emergent political patterns.

## 3.3 Objectives & Bounties
- **Definition:** Objectives are tasks with rewards, posted by players or NPCs.
- **Types:**
 - Military: Defend, blockade, capture, assassinate.
 - Economic: Deliver goods, secure trade routes, establish outposts.
 - Political: Build alliances, influence leaders, spread propaganda.
 - Personal: Retrieve items, perform favors, protect individuals.
- **Reward Types:**
 - Currency (Wealth)
 - Abstract (Favors owed, Reputation boosts, Fame gains)
- **Hierarchy:**
 - High-level objectives can spawn sub-objectives.
 - NPCs at various levels choose which objectives to pursue based on incentives and compatibility.

## 3.4 Progression & Upgrades
- **Character Sheet:**
 - Displays currencies (Wealth, Reputation, Fame, Favors).
 - Lists Traits (active and passive).
 - Shows Relationships (trust, fear, favor debts, hostility).
- **Upgrades:**
 - Purchased with currencies or rewarded through completed objectives.
 - Traits act as the primary means of improvement (skills, bonuses, narrative flavor).
 - Some upgrades require being at specific locations (e.g., shipyards, academies, corporate HQs).

## 3.5 Resources / Currencies
- **Wealth:** Money and material goods used for bounties and upgrades.
- **Reputation:** Measure of reliability and status among peers.
- **Fame:** Public recognition - affects how widely known the player is.
- **Favors:** Abstract debts that can be called in for influence or rewards.

---

# 4. World & Map

## Sector Structure
- **Scale:** ~12 star systems
- **Each system:**10-20 locations (planets, stations, colonies, asteroids)

## Generation
- Sector procedurally generated at game start.
- Locations may be hand-crafted templates populated procedurally.

## Factions
- Up to 2-3 of each type (Government, Corporation, Criminal, Religious, Other).
- Factions vary per game for replayability.

## Map Interface
- **Abstract Node Network:** Locations are connected by defined routes.
- **Layered UI:**
 - Faction control overlay
 - Trade/influence lines
 - Character presence indicators
- **Board Game Aesthetic:** Minimalistic, functional design emphasizing clarity and system readability.

---

# 5. Tone & Narrative

- **Tone:** Serious but adventurous — moral ambiguity, opportunism, and survival at the frontier.
- **Narrative Layer:**
 - Initially minimal and systemic.
 - Later expanded with short narrative events tied to relationships and factions.
 - Events remain reactive and secondary to emergent simulation.

---

# 6. Future Expansion / Stretch Goals

- **Story Arcs:** Layer overarching narrative threads that tie into simulation (e.g., sector crisis, alien discovery).

---

# 7. Target Experience Summary

The player feels like a cunning operator in a living, reactive world — a strategist who bends people and systems to their will through influence and foresight, not brute force. Watching the simulation tick forward should feel like handling a complex, satisfying machine that responds intelligently to every push and pull.

---

# Mechanics Summary

## 1. Overview
This document formalizes how major systems operate numerically and procedurally. It describes:
- Core simulation loop
- Objective (bounty) resolution
- NPC decision-making
- Trait effects and acquisition
- Currency flow
- Relationships and favor systems

The aim is a mechanically legible simulation that produces emergent political and economic stories.

## 2. Simulation Core

### 2.1 Time Structure
- **Granularity:**1 tick =1 day.
- **Progression:** The game runs until an interrupt condition is met:
 - Player-set interval (e.g., max7 days).
 - Event triggers (objective completed, threat, communication, etc.).
- **Process Order (per tick):**
1. Update location events (trade, population changes, economic values).
2. Update NPC goals & decision trees.
3. Resolve active objectives.
4. Update relationships, traits, and world state.
5. Check for player or system interrupts.

## 3. NPC System

### 3.1 Behavior Loop (per tick)
1. **Evaluate Needs:** If idle or low priority tasks exist, generate new objectives.
2. **Select Objective:** Using weighted evaluation:
 - Reward potential
 - Personal goals alignment
 - Risk tolerance (from traits)
 - Relationship considerations
3. **Act:** Move to location(s), perform actions, interact with other NPCs.
4. **React:** Update attitudes toward those who helped/hindered.
5. **Adapt:** Add/remove traits or modify goals based on outcomes.

### 3.2 Trait System
Traits modify behavior probabilities, stats, and relationships.
- **Categories:**
 - Personality: Ambitious, Cautious, Vengeful, Altruistic
 - Skill: Diplomat, Trader, Commander, Explorer
 - Status: Heroic, Infamous, Corrupt, Visionary

## 4. Player Systems

### 4.1 Character Sheet
UI displays:
- Name, role, faction
- Currencies
- Traits (sortable by category)
- Relationships graph (interactive network view)
- Objectives tab (created, accepted, completed)

### 4.2 Player Actions
- **Post Objective:** Create new task with bounty/reward.
- **Accept Objective:** Undertake existing objective personally.
- **Negotiate Favor:** Trade wealth, information, or help for favors.
- **Move:** Travel to a connected node.
- **Wait / Advance Time:** Let the world progress until interrupt.
- **Inspect NPC / Faction:** Review data sheets and relationships.

---

## 5. World Generation

### 5.1 Sector Generation
- Choose total system count (e.g.,12).
- For each system, generate10–20 locations:
 - Type (planet, station, colony)
 - Local economy template
 - Owner faction
 - Environmental flavor text
- Distribute factions:
 -2–3 per type (Government, Corporate, Criminal, Religious, Other)
 - Assign home systems, relationships, rivalries.

### 5.2 Starting Conditions
- Player starts in neutral or minor-faction-aligned position.
-50–100 NPCs generated with traits, roles, and affiliations.
- World begins with small number of posted objectives to “seed” simulation.

---

## 6. Example Simulation Flow

**Day 1:**
- Player posts a trade objective: “Deliver supplies to Outpost Theta” (reward:500 credits).
- Local merchant NPC sees high utility and accepts.

**Day 3:**
- Merchant attacked by pirates (autonomous NPC objective).
- Player receives event notification.

**Day 4:**
- Player hires mercenary via another bounty: “Protect Merchant Convoy.”
- Mercenary and pirates engage; outcome changes reputation web.

**Day 6:**
- Trade completed successfully ? merchant gains wealth and gratitude ? player gains reputation, favor owed.
- Pirate faction reputation drops; economy of Theta stabilizes.

*The player never directly controlled anyone, but their incentives shaped a miniature story of trade, danger, and influence.*

---

# UI Summary

## 1. Design Goals
- **Readability First:** The player should see the state of the world at a glance. Every number, faction, and relationship must be visually accessible.
- Tactile "Toybox" Feel:** Interactions (clicks, drags, panning) should feel like moving pieces on a tabletop rather than controlling a video game UI.
- **Board Game Aesthetic:** Flat map, strong iconography, limited color palette, sharp typography, and layered information panels.
- **Interrupt-Based Flow:** Player actions open compact panels instead of switching screens — maintaining situational awareness of the map.

## 2. Primary Screens

### 2.1 Sector Map (Main Screen)
The heart of the game.

**Purpose:** Show the entire frontier sector — systems, routes, faction control, and NPC activity.

**Layout (visual mockup description):**
```
???????????????????????????????????????????????
? TOP BAR ?
? [Day124] [Wealth:2300?] [Reputation:52] ?
? [Fame:8] [Favors:2 owed |1 owing] ?
???????????????????????????????????????????????
? ?
? ? Solace Station ? Port Haven ?
? ? Trade Route ? ?
? ? Dustfall Mining Hub ?
? ?
? (Icons for NPCs visible as portraits or initials) ?
? ?
? [Faction Territories Overlaid with subtle tint] ?
? ?
???????????????????????????????????????????????
? BOTTOM PANEL ?
? [Post Objective] [Advance Time] [Messages] ?
? [Character Sheet] [Map Filters ?] ?
???????????????????????????????????????????????
```

**Interactions:**
- Left-click a location ? opens location panel.
- Right-click a location ? create objective targeting that location.
- Mouse-over nodes ? tooltip summary (faction, population, trade value).
- Zoom/pan ? entire sector view vs individual system view.
- Filters: toggle overlays for factions, trade, influence, NPC presence.

### 2.2 Location Panel
Appears as a slide-up card from the bottom or right side of the screen.

**Shows:**
- Location name, faction, economy indicators.
- Active objectives and events.
- List of visiting NPCs and their current actions.
- Button: “Create Objective Here”
- Mini-map inset showing local routes.

Visual feel: compact info card, like a board game reference sheet.

### 2.3 Character Sheet (Player & NPCs)
Modal panel accessed via button or clicking an NPC.

**Sections:**
```
?????????????????????????????????????????????
? [Portrait/Emblem] [Name] ?
? Role: Captain | Faction: Neutral ?
?-------------------------------------------?
? Wealth:2,300? | Reputation:52 | Fame:8 ?
?-------------------------------------------?
? TRAITS ?
? - Diplomat (+10% success on negotiations) ?
? - Cautious (-10% risk on high danger ops) ?
?-------------------------------------------?
? RELATIONSHIPS GRAPH (mini-network) ?
? [Player]??(trust45)??[NPC X] ?
? [NPC Y]??(fear20)???????????????????????
?-------------------------------------------?
? OBJECTIVES ?
? - “Protect Merchant Convoy” (in progress) ?
? - “Attend Governor’s Banquet” (accepted) ?
?????????????????????????????????????????????
```

**Interactions:**
- Hover traits ? tooltip showing numeric effects.
- Click relationship nodes ? jump to NPC sheet.
- Drag graph background ? explore wider relationship web.

### 2.4 Objective Board
Functions like a marketplace or job board for tasks.

**Layout:**
| Objective | Issuer | Reward | Type | Status |
|--------------------------|---------------|--------|----------|-------------|
| Deliver ore to Solace Station | NPC12 |400? | Trade | Open |
| Patrol Dustfall route | Military Cmd. | Favor | Military | Open |
| Smuggle data | Criminal Syndicate |500? | Covert | In Progress |

- Filter by type (trade, political, military, etc.)
- Sort by reward, issuer, or proximity.
- Click to view or accept objective.
- Post button opens objective creation form.
- Visual: reminiscent of a spreadsheet or mission board pinned on a corkboard.

### 2.5 Relationship Network View
A toggle overlay on the main map or standalone modal.

**Design:**
- Nodes = characters or factions.
- Edges = relationships (colored lines).
 - Green = trust
 - Blue = respect
 - Red = hostility
 - Gold = favor owed

**Interactions:**
- Hover node ? quick stats (wealth, fame, faction).
- Click ? open character sheet.
- Drag ? reposition clusters manually (toy-like feel).

This view emphasizes the social web simulation, allowing players to see influence rippling outward.

### 2.6 Event Log / Message Feed
Compact sidebar or bottom ticker.

**Types of messages:**
- Objective completions
- NPC requests or messages
- Relationship changes
- Economic fluctuations

**Example:**
```
[Day122] Mercenary Juno failed to defend Dustfall Convoy.
[Day123] You gained1 Favor from Merchant Tyrell.
[Day124] Corporate Syndicate expanded into Port Haven.
```

- Filter toggles: [Personal] [Faction] [Economy] [All].
- Clickable entries jump to relevant map nodes or NPCs.

---

## 3. Style Guide (Preliminary)
- **Color Palette:** Muted blues, metallic grays, warm gold accents. Each faction has a unique tint overlay.
- **Typography:** Geometric sans-serif (e.g., Rajdhani, Orbitron) for headers; clean mono or semi-serif for body.
- **Icons:** Simplified symbols for roles (pilot, trader, politician, soldier).
- **Motion:** Minimal — card slides, fades, and slight hover motion.
- **Audio UX:** Light “tactile” clicks, hums, and soft pings for notifications.
- **General Feel:** Calm, analytical, but full of potential — like adjusting knobs on a sophisticated machine.

---

## 4. Interaction Flow (Example Scenario)

1. Player opens game ? sees Sector Map.
2. Several NPC icons moving; objectives visible as small markers.
3. Player clicks Port Haven ? Location Panel opens.
4. Notices trade instability ? clicks “Create Objective.”
5. Objective Creation Form:
 - Type: “Deliver Supplies”
 - Reward:400?
 - Expiry:10 days
6. Confirms ? Objective Board updates ? visible to NPCs.
7. Player advances time.
8. Watches small events in Event Log.
9. Map pulses briefly as objectives are taken and resolved.
10. Notification: “Merchant Lira fulfilled your contract.”
11. Character Sheet updates ? Favor +1, Wealth -400?, Reputation +3.

*Everything happens in-place — the player never loses sight of the board.*

---

## 5. Secondary Panels (Future Iterations)
- **Faction Dashboard:** Shows faction traits, goals, relationships. Acts like a "macro" version of NPC sheets.
- **Economic Overview:** Graph of trade flows, blockades, and system stability trends.
- **Journal / Summary Screen:** Long-term record of the player's influence and major events.

---

