# 🌌 Reality Engine v1.4
**A Universal, Logic-Driven Generation Suite for Worldbuilders, GMs, and Authors**

The Reality Engine is a highly advanced, text-based terminal application designed to generate infinitely complex characters, starships, planets, factions, and items. By feeding the engine custom `.json` "Universe Files," users can create anything from a gritty Cyberpunk street-rat to a galaxy-spanning Sci-Fi empire.



## ✨ Core Features
* **Interactive Terminal UI:** A sleek, arrow-key-driven interface. No clunky command-line typing required.
* **Smart Narrative Engine:** The engine doesn't just spit out stats; it weaves them together into grammatically correct, context-aware paragraphs. It even dynamically adjusts pronouns (e.g., using "it/its" for spaceships and AI).
* **The Architect Wizard:** Build complex JSON universe foundations directly inside the app without ever opening a code editor.
* **Pre-Flight Validator:** A built-in diagnostic tool that mathematically tests your custom universes for logic loops, impossible point-pool budgets, and "orphaned" items before you ever roll a character.
* **Deep Stat & Tag Gating:** Items can grant hidden "Tags" which unlock specific narrative sentences or restrict future gear rolls.

---

## 📁 Installation & Folder Structure
No installation required! Just run `RealityEngine.exe`. 

On its first launch, the engine will automatically generate its required ecosystem in the same folder:
* `/UniverseFiles/` - Drop your custom `.json` modules here.
* `/Saves/` - All generated entities you choose to save will be neatly archived here as `.txt` files.
* `/Docs/` - Contains the internal Help Library.

---

## 🎮 How to Use (For Players)
1. **Load a Universe:** Select `Load Universe Module` from the main menu and pick a world. The engine will instantly calculate the total possible combinations (often in the billions or trillions).
2. **Generate Entity:** Choose an Archetype (e.g., "Orbital Marine" or "Capital Starship") and watch the engine roll stats, gear, and biography.
3. **Debug Mode:** If you want to see exactly *how* the engine made its choices behind the scenes, toggle `Debug Mode [ON]` from the main menu. It will show you every dice roll, tag applied, and blocked item in real-time.
4. **Save:** Press `S` on the final character sheet to archive your creation.

---

## 🛠️ Modding Guide (For Worldbuilders)
The true power of the Reality Engine lies in its custom JSON Universe files. You can use the in-app **Architect Mode** to build a starting template, and then open the JSON file in any text editor to add extreme depth.

### 1. Tag Logic (`GrantsTag` & `Requires`)
You can link items and lore together using the tagging system.
* If a player rolls a "Powered Exosuit", you can give it `"GrantsTag": "Heavy_Armor"`.
* You can then make a Heavy Plasma Cannon that has `"Requires": "Heavy_Armor"`. 
* *Result:* Only characters strong enough to roll the Exosuit can roll the Plasma Cannon.

### 2. Stat-Gated Narratives (`MinStatName` & `MinStatValue`)
Biographies are stitched together from arrays of sentences. You can lock specific sentences behind stat checks.

{ 
  "Text": "Their reflexes are a blur, dodging bullets with ease.", 
  "MinStatName": "Reflexes", 
  "MinStatValue": 16, 
  "Weight": 50 
}

Result: This sentence will only be injected into the character's backstory if their randomly rolled Reflexes stat is 16 or higher.
3. Smart Placeholders
When writing your BioTemplates, use these placeholders to let the engine inject the character's data dynamically:
{First} - First Name
{Last} - Last Name
{Nick} - Nickname
{Full} - Full Name & Nickname combined
{Subject} - He / She / They / It
{Object} - Him / Her / Them / It
{Possessive} - His / Her / Their / Its
{Trait Name} - i.e., {Primary Weapon} or {Armor Class} will print the exact item rolled.
⚠️ Troubleshooting
"Math Error" Warning on Load: Your PointPool total budget is higher than the maximum possible stats. Lower your TotalPoints or increase MaxPerAttribute.
"Dead Item" Warning on Load: You have an item that Requires a tag, but absolutely nothing in your JSON GrantsTag for it. The Validator caught it so your game doesn't break!
Items not appearing: Check your spelling! Tags are case-sensitive. Heavy_armor is not the same as Heavy_Armor.
Built with C# | Designed for infinite creativity.
