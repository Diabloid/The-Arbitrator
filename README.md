# The Arbitrator 🗡️

**The Arbitrator** is a 2D Action-RPG set in a dark fantasy world, built with the Unity engine. 

This project started as an educational internship (a practical assignment of a 3rd-year Software Engineering student), but quickly grew into something more. It's not just a boilerplate code submission "for the sake of a grade", but a game infused with a lot of soul, sleepless nights, and love for the action-RPG genre. 

Currently, the game is presented at the **MVP** stage.

## What is an MVP and why this approach?
**MVP (Minimum Viable Product)** stands for exactly that. In the context of game development, it means the game doesn't feature dozens of hours of content, hundreds of weapon types, or convoluted quests. Instead, it implements a **Core Gameplay** loop that works stably and beautifully:
* Core movement mechanics (including dodge rolls).
* A combat system with damage calculation and interface-based interactions. Combo attacks.
* Fully-fledged enemy AI.
* One, but thoroughly polished, final boss.

This is enough to showcase the technical level and the overall "vibe" of the game without stretching the development over years.

---

## 🛠 Technical Stack & Architecture
The game isn't simply "assembled from blocks", but features a well-thought-out engineering structure written in C#:

* **Behavior Trees:** Modular artificial intelligence for standard enemies. They can patrol the area, spot the player (Field of View), chase, and attack, switching states without spaghetti code.
* **Finite State Machine (FSM):** Designed specifically for the final boss — Maurice the Necromancer. The battle is divided into 5 distinct phases, each with its own logic, attacks, and transition conditions.
* **Singleton Pattern:** Global managers, such as `AudioManager` and `SaveManager`, which exist as single instances and handle audio streams (with disk streaming) and cross-scene saves.
* **Data Persistence (JSON):** The player progress saving system is implemented via data serialization into the JSON format.
* **Interfaces:** Interaction between objects (e.g., dealing damage with a sword) is handled through the universal `IDamageable` interface, allowing easy addition of new object types without altering weapon code.
* **Adaptive UI:** The interface scales correctly for various screen resolutions using Canvas Scaler.

---

## 🎮 How to play?
1. Go to the **Releases** section (on the right side of this page).
2. Download the latest archive `The Arbitrator_FinalBuild v1.0.zip`.
3. Extract it into any convenient folder on your PC.
4. Run the **The Arbitrator.exe** file (with the knight icon) and enjoy!

Thank you for your attention and for playing!
