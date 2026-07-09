# Game Demo: RDR2-Inspired Action Adventure

[![Engine](https://img.shields.io/badge/Unity-2022.3%2B-black.svg?logo=unity)](https://unity.com/)
[![Language](https://img.shields.io/badge/C%23-HLSL-blue.svg)](#)
[![Status](https://img.shields.io/badge/Status-In%20Development-orange.svg)](#)

This is a personal action-adventure game demo developed in Unity, heavily inspired by the immersive world and mechanics of *Red Dead Redemption 2*. The ultimate goal of this project is to build a seamless open-world experience encompassing four distinct districts, complete with robust player controllers and dynamic NPC interactions.

---

## Core Features & Roadmap

* **World Building:** Planned development of four unique districts, each with specific environmental storytelling and level design.
* **Advanced Animation Systems:** Focus on natural character expressions and fluid state transitions. Currently implementing and optimizing complex animation blending, particularly the smooth transition between standard movement and weapon-handling states.
* **Technical Implementation:** Extensive use of C# Coroutines and delayed calls to resolve animation stiffness and manage asynchronous game logic.
* **Atmospheric Audio:** Exploring a dark, atmospheric soundscape with traditional instrumental influences to enhance the immersion and tension of combat and exploration.

---

## Gallery & Development Progress

### Engine & Workflow
A look into the Unity editor, showcasing the scene hierarchy and development environment.
![](/images/1.png)
<br><br>

### World Environment
In-game scene captures highlighting the lighting, terrain, and environmental shaders.
![](/images/3.png)
<br><br>

### Character & AI Interactions
Showcasing the player character model and dynamic interactions with scene NPCs.
![](/images/2.png)
<br><br>

![](/images/4.png)
<br><br>

---

## Tech Stack

* **Game Engine:** Unity (Recommended version: `2022.x`)
* **Programming:** C# (Object-Oriented Design, Coroutines, State Machines)
* **Graphics & Rendering:** HLSL, Unity Shader Graph, Custom Rendering Pipelines
* **Animation:** Unity Animator, Animation Rigging, Blend Trees

---

## Local Deployment & Setup

To run or modify this project locally on your machine, follow these steps:

1.  **Install Unity:** Download and install the [Unity Hub](https://unity.com/download). Install a **Unity 2022** version (matching the exact project version is highly recommended to avoid package conflicts).
2.  **Clone the Repository:**
    ```bash
    git clone [https://github.com/your-username/your-repo-name.git](https://github.com/your-username/your-repo-name.git)
    ```
3.  **Open the Project:** Launch Unity Hub, click on `Add project from disk`, and select the cloned repository folder.
4.  **Play:** Once the editor finishes importing assets and compiling scripts, open the main scene in the `Scenes` folder and hit the Play button.