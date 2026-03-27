using UnityEngine;
using UnityEditor;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    /// <summary>
    /// Generates the AnastasiaDialogueDatabase.asset with all 112 lines
    /// from docs/18_PRINCESS_ANASTASIA.md.
    ///
    /// Menu: Tartaria > Build Assets > Anastasia Dialogue Database
    /// </summary>
    public static class AnastasiaDialoguePopulator
    {
        const string AssetPath = "Assets/_Project/Config/AnastasiaDialogue.asset";

        [MenuItem("Tartaria/Build Assets/Anastasia Dialogue Database", false, 20)]
        public static void BuildDialogueDatabase()
        {
            var db = ScriptableObject.CreateInstance<AnastasiaDialogueDatabase>();
            db.lines = CreateAllLines();

            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Config"))
                AssetDatabase.CreateFolder("Assets/_Project", "Config");

            AssetDatabase.CreateAsset(db, AssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[Tartaria] Anastasia Dialogue Database created: {db.lines.Length} lines at {AssetPath}");
            Selection.activeObject = db;
        }

        static AnastasiaLine[] CreateAllLines()
        {
            return new AnastasiaLine[]
            {
                // ══════════════════════════════════════════════
                // Moon 1 — Magnetic Moon (Echohaven) — 6 lines
                // ══════════════════════════════════════════════
                Line(0,  1, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "This dome. I watched them build it. Ages ago. They sang while they worked."),
                Line(1,  1, AnastasiaLineCategory.LoreWhisper,       "restoration",
                    "The stones remember their shape. You just reminded them."),
                Line(2,  1, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "Your resonance is... different. Brighter than what the Archive expected."),
                Line(3,  1, AnastasiaLineCategory.CompanionReaction, "companion_milo",
                    "He's louder than anyone I've heard in centuries. I think I like it."),
                Line(4,  1, AnastasiaLineCategory.MemoryFragment,    "discovery",
                    "Careful with the lower strata. That's where the important things sleep."),
                Line(5,  1, AnastasiaLineCategory.PersonalReflection,"idle",
                    "At night, it almost looks like it used to."),

                // ══════════════════════════════════════════════
                // Moon 2 — Lunar Moon (Crystalline Caverns) — 7 lines
                // ══════════════════════════════════════════════
                Line(6,  2, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "The Archive looks like this. All light and facets. But colder."),
                Line(7,  2, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "These crystals amplify Aether the way a bell amplifies silence."),
                Line(8,  2, AnastasiaLineCategory.MemoryFragment,    "discovery",
                    "I've been in a cave of light for so long, I'd forgotten what darkness smells like."),
                Line(9,  2, AnastasiaLineCategory.BuildingCommentary,"tuning_complete",
                    "You're better at that than the original tuners were. Don't tell them I said so."),
                Line(10, 2, AnastasiaLineCategory.CompanionReaction, "companion_lirael",
                    "She sings to the crystals. They sing back. I wonder if she knows she's doing it."),
                Line(11, 2, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "This was the heart of their resonance network. The first antenna."),
                Line(12, 2, AnastasiaLineCategory.PersonalReflection,"idle",
                    "If you put your ear very close... no. I suppose you can't hear it."),

                // ══════════════════════════════════════════════
                // Moon 3 — Electric Moon (Windswept Highlands) — 8 lines
                // ══════════════════════════════════════════════
                Line(13, 3, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "Wind. I remember wind. It's one of the first things you forget when you become light."),
                Line(14, 3, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "The orphan train tracks... those children. They built this route themselves."),
                Line(15, 3, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "Highland architecture is different. Built to bend, not break. Like reeds."),
                Line(16, 3, AnastasiaLineCategory.BuildingCommentary,"restoration",
                    "The turbines used to power the aqueducts. Water and wind, partners."),
                Line(17, 3, AnastasiaLineCategory.CompanionReaction, "companion_thorne",
                    "The airship captain carries something heavy. Not cargo."),
                Line(18, 3, AnastasiaLineCategory.CompanionReaction, "companion_npc",
                    "Children echoes are the cruelest thing about this world. They don't know they're trapped."),
                Line(19, 3, AnastasiaLineCategory.MemoryFragment,    "weather_storm",
                    "Lightning and Aether were twins once. The architects used both."),
                Line(20, 3, AnastasiaLineCategory.PersonalReflection,"idle",
                    "I can see four zones from here. Four incomplete worlds. You'll fix them."),

                // ══════════════════════════════════════════════
                // Moon 4 — Self-Existing Moon (Star Fort Bastion) — 9 lines
                // ══════════════════════════════════════════════
                Line(21, 4, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "I know this place. Not this specific fort, but this geometry. I grew up in one like it."),
                Line(22, 4, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "Star forts weren't military. They were amplifiers. Five points, each resonating at a different frequency."),
                Line(23, 4, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "The moat isn't water anymore. It's liquid corruption. Be careful."),
                Line(24, 4, AnastasiaLineCategory.StoryBeat,         "climax",
                    "They're trying to destroy what they can't corrupt. That's desperation."),
                Line(25, 4, AnastasiaLineCategory.MemoryFragment,    "restoration",
                    "The fort held. Eight hundred years of neglect and it still held."),
                Line(26, 4, AnastasiaLineCategory.CompanionReaction, "companion_korath",
                    "He remembers the old world better than I do. But he carries it differently -- like a wound that taught him something."),
                Line(27, 4, AnastasiaLineCategory.BuildingCommentary,"building",
                    "Golden ratio alignments were mandatory in royal architecture. Not for beauty -- for function."),
                Line(28, 4, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "A Star Chamber. Where the Archive was accessed. Where I was... stored."),
                Line(29, 4, AnastasiaLineCategory.PersonalReflection,"idle",
                    "Watching the horizon from a star fort. Some things transcend timeline."),

                // ══════════════════════════════════════════════
                // Moon 5 — Overtone Moon (Sunken Colosseum) — 9 lines
                // ══════════════════════════════════════════════
                Line(30, 5, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "A colosseum! Not for fighting -- for resonance performances. The acoustics must be extraordinary."),
                Line(31, 5, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "The seating holds five thousand echo imprints. Five thousand people who watched something beautiful."),
                Line(32, 5, AnastasiaLineCategory.LoreWhisper,       "restoration",
                    "The stage is alive again. I can hear the ghost of applause."),
                Line(33, 5, AnastasiaLineCategory.StoryBeat,         "tuning_complete",
                    "Overtones were sacred. The note above the note -- the truth behind the truth."),
                Line(34, 5, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "Water doesn't corrupt -- it preserves. What's beneath the surface may be more intact than what's above."),
                Line(35, 5, AnastasiaLineCategory.CompanionReaction, "companion_milo",
                    "He's trying to sell tickets to an echo audience. I admire the optimism."),
                Line(36, 5, AnastasiaLineCategory.MemoryFragment,    "combat_end",
                    "The corruption in water is different. Slower. Patient. Like venom."),
                Line(37, 5, AnastasiaLineCategory.PersonalReflection,"idle",
                    "If I stood here and sang... would you hear me? Or just the light?"),
                Line(38, 5, AnastasiaLineCategory.BuildingCommentary,"building",
                    "Amphitheater acoustics require precise curvature. The golden ratio isn't just visual."),

                // ══════════════════════════════════════════════
                // Moon 6 — Rhythmic Moon (Living Library) — 10 lines
                // ══════════════════════════════════════════════
                Line(39, 6, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "Books. Real books. With pages that turn. I haven't touched a page in..."),
                Line(40, 6, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "The Living Library wasn't just storage. It was an interface to the Archive. My Archive."),
                Line(41, 6, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "I can feel the Archive here. Like pressing your hand against glass and feeling warmth on the other side."),
                Line(42, 6, AnastasiaLineCategory.MemoryFragment,    "discovery",
                    "The librarians encoded knowledge in rhythm. Read with your ears, not your eyes."),
                Line(43, 6, AnastasiaLineCategory.CompanionReaction, "companion_npc",
                    "Veritas. Still at the keys. Still playing the piece that was interrupted. Still waiting for the ending."),
                Line(44, 6, AnastasiaLineCategory.StoryBeat,         "restoration",
                    "The Library is remembering. Sections are lighting up that have been dark since the Flood."),
                Line(45, 6, AnastasiaLineCategory.CompanionReaction, "companion_lore",
                    "That shelf describes my preservation. In clinical terms. As if archiving a person is a simple administrative act."),
                Line(46, 6, AnastasiaLineCategory.PersonalReflection,"idle",
                    "I used to read by candlelight. Light reading light. There's a joke in there but it makes me tired."),
                Line(47, 6, AnastasiaLineCategory.BuildingCommentary,"building",
                    "Library wings should follow Fibonacci sequencing. Each room slightly larger than the last."),
                Line(48, 6, AnastasiaLineCategory.StoryBeat,         "climax",
                    "The Library is breathing. The whole structure is alive again. I can feel every page turning."),

                // ══════════════════════════════════════════════
                // Moon 7 — Resonant Moon (Clockwork Citadel) — 9 lines
                // ══════════════════════════════════════════════
                Line(49, 7, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "Clockwork. Gears that mesh and turn. Do you know what I'd give for a single moving part?"),
                Line(50, 7, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "The Citadel's orrery mapped every celestial body. Tartarian astronomy was mechanical prophecy."),
                Line(51, 7, AnastasiaLineCategory.MemoryFragment,    "restoration",
                    "That sound -- metal touching metal, turning with purpose. It's the most physical sound in this world."),
                Line(52, 7, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "This machine predicted the Mud Flood. The architects who read it had four hours to prepare. I was archived in three."),
                Line(53, 7, AnastasiaLineCategory.CompanionReaction, "companion_milo",
                    "He keeps poking the gears. He's going to lose a finger and somehow monetize it."),
                Line(54, 7, AnastasiaLineCategory.StoryBeat,         "tuning_complete",
                    "Synchronization was the Tartarian word for prayer. Making things move together."),
                Line(55, 7, AnastasiaLineCategory.MemoryFragment,    "combat_end",
                    "Corrupted machines aren't evil. They're in pain. Gears grinding against their own design."),
                Line(56, 7, AnastasiaLineCategory.PersonalReflection,"idle",
                    "Tick. Tick. Tick. Time moves here. In the Archive, there is no tick. Only... duration."),
                // Note: Moon 7 has a combat [Invisible] marker in doc — counted as no-line

                // ══════════════════════════════════════════════
                // Moon 8 — Galactic Moon (Verdant Canopy) — 8 lines
                // ══════════════════════════════════════════════
                Line(57, 8, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "Things grow here. Roots push through stone. Life refusing to yield. I envy roots."),
                Line(58, 8, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "Bioluminescence is Aether expressing itself through biology. Even nature builds with sacred ratios."),
                Line(59, 8, AnastasiaLineCategory.MemoryFragment,    "restoration",
                    "The vines knew where to go. They were waiting for permission."),
                Line(60, 8, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "I can't smell them. That's the cruelest part. I can see beauty but I can't... participate in it."),
                Line(61, 8, AnastasiaLineCategory.CompanionReaction, "companion_lirael",
                    "Lirael touches the bark and the tree glows brighter. She doesn't know she's feeding it."),
                Line(62, 8, AnastasiaLineCategory.MemoryFragment,    "discovery",
                    "The root network mirrors the ley lines above. Nature copied the architects. Or maybe the other way around."),
                Line(63, 8, AnastasiaLineCategory.BuildingCommentary,"building",
                    "Build with the trees, not against them. The strongest structures let life grow through them."),
                Line(64, 8, AnastasiaLineCategory.PersonalReflection,"idle",
                    "Sunlight passes through me. I'm too transparent even for a shadow."),

                // ══════════════════════════════════════════════
                // Moon 9 — Solar Moon (Auroral Spire) — 9 lines
                // ══════════════════════════════════════════════
                Line(65, 9, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "Aurora. The sky remembering how to sing in color. This is the frequency I was preserved in."),
                Line(66, 9, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "The Spire channels solar energy into Aether. Pure conversion. I am the same kind of conversion."),
                Line(67, 9, AnastasiaLineCategory.StoryBeat,         "tuning_complete",
                    "I can feel the aurora tuning like a voice inside my chest. If I had a chest."),
                Line(68, 9, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "From up here, the ley lines look like veins. The planet has a circulatory system and we're teaching it to pulse again."),
                Line(69, 9, AnastasiaLineCategory.CompanionReaction, "companion_thorne",
                    "He sees navigation. I see home. Same light, different meaning."),
                Line(70, 9, AnastasiaLineCategory.MemoryFragment,    "restoration",
                    "For a moment, the aurora and I were the same frequency. I couldn't tell where I ended and the sky began."),
                Line(71, 9, AnastasiaLineCategory.MemoryFragment,    "combat_end",
                    "Corruption fears light. It's the one thing they can't absorb. That gives me hope."),
                Line(72, 9, AnastasiaLineCategory.PersonalReflection,"idle",
                    "At night, I'm almost invisible against the aurora. Two kinds of light, overlapping."),
                // Note: Moon 9 combat is [Invisible] -- no line

                // ══════════════════════════════════════════════
                // Moon 10 — Planetary Moon (Deep Forge) — 8 lines
                // ══════════════════════════════════════════════
                Line(73, 10, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "The Forge heat... I can sense it even though I can't feel warmth. Like a memory of temperature."),
                Line(74, 10, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "Sonic hammering. The Tartarian smiths shaped metal with sound, not muscle. Each strike a note."),
                Line(75, 10, AnastasiaLineCategory.MemoryFragment,    "restoration",
                    "The anvil's ring -- B flat. The note of transformation. Matter yielding to intent."),
                Line(76, 10, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "This alloy responds to Aether. The smiths sang to their creations. The creations listened."),
                Line(77, 10, AnastasiaLineCategory.CompanionReaction, "companion_korath",
                    "He's remembering the old forges. The ones that made the tools that built the world I lost."),
                Line(78, 10, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "The planet's core is pure frequency. We're building on top of the universe's bass note."),
                Line(79, 10, AnastasiaLineCategory.BuildingCommentary,"building",
                    "Forged Tartarian alloy resonates for centuries. These buildings will outlast any flood."),
                Line(80, 10, AnastasiaLineCategory.PersonalReflection,"idle",
                    "Fire is the only element that creates light. We have that in common."),

                // ══════════════════════════════════════════════
                // Moon 11 — Spectral Moon (Tidal Archive) — 8 lines
                // ══════════════════════════════════════════════
                Line(81, 11, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "Water and Archives have the same function -- preserving what was. But water doesn't choose what it keeps."),
                Line(82, 11, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "The Tidal Archive is older than the planetary one. Older than the civilization that built me."),
                Line(83, 11, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "I float through water the way I float through everything. No resistance. No... interaction."),
                Line(84, 11, AnastasiaLineCategory.StoryBeat,         "discovery",
                    "A whole city. Intact. Under the surface. They chose to let the tide take them rather than fight it."),
                Line(85, 11, AnastasiaLineCategory.MemoryFragment,    "discovery",
                    "The tide writes in light. I can almost read it. Almost."),
                Line(86, 11, AnastasiaLineCategory.MemoryFragment,    "restoration",
                    "The water is clearing. I can see all the way down. All the way to the old world."),
                Line(87, 11, AnastasiaLineCategory.CompanionReaction, "companion_lirael",
                    "Lirael stands at the waterline. The tide reaches for her and passes through. We both know how that feels."),
                Line(88, 11, AnastasiaLineCategory.PersonalReflection,"idle",
                    "Tides come and go. Echoes stay. I'm not sure which is lonelier."),

                // ══════════════════════════════════════════════
                // Moon 12 — Crystal Moon (Celestial Observatory) — 8 lines
                // ══════════════════════════════════════════════
                Line(89, 12, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "The Observatory mapped the stars that map us. Everything is geometry at sufficient distance."),
                Line(90, 12, AnastasiaLineCategory.LoreWhisper,       "discovery",
                    "These lenses were ground to Tartarian precision -- twelve decimal places. The stars demanded nothing less."),
                Line(91, 12, AnastasiaLineCategory.MemoryFragment,    "discovery",
                    "I recognize these constellations. They've shifted since I was archived. Even the sky moved on without me."),
                Line(92, 12, AnastasiaLineCategory.StoryBeat,         "restoration",
                    "432 Hz. The frequency that connects the planet to the cosmos. The frequency of home."),
                Line(93, 12, AnastasiaLineCategory.CompanionReaction, "companion_npc",
                    "The navigator sees patterns I can feel but not name. Mathematics made manifest."),
                Line(94, 12, AnastasiaLineCategory.LoreWhisper,       "restoration",
                    "The planet can see the sky again. And the sky can see us."),
                Line(95, 12, AnastasiaLineCategory.BuildingCommentary,"building",
                    "Observatory construction requires absolute stillness. No vibration. Not even from the wind."),
                Line(96, 12, AnastasiaLineCategory.PersonalReflection,"idle",
                    "I used to wish on stars. Now I'm the same kind of light. I wish on sunrises instead."),

                // ══════════════════════════════════════════════
                // Moon 13 — Cosmic Moon (Planetary Nexus) — 5 lines
                // ══════════════════════════════════════════════
                Line(97,  13, AnastasiaLineCategory.StoryBeat,         "discovery",
                    "All the ley lines. Every one. Connected. I can feel the whole planet. It's... awake."),
                Line(98,  13, AnastasiaLineCategory.StoryBeat,         "restoration",
                    "This is what the Archive preserved us for. This moment. All the silence was for this."),
                Line(99,  13, AnastasiaLineCategory.MemoryFragment,    "pre_climax",
                    "Whatever happens next -- the digging, the tuning, the building, the quiet afternoons -- it mattered. All of it."),
                Line(100, 13, AnastasiaLineCategory.StoryBeat,         "climax",
                    "Listen. The world is singing. After eight hundred years of silence. It's singing."),
                Line(101, 13, AnastasiaLineCategory.PersonalReflection,"post_climax",
                    "...thank you. For bringing it all back. For bringing me back. Almost."),

                // ══════════════════════════════════════════════
                // Day Out of Time — 2 lines
                // ══════════════════════════════════════════════
                Line(102, 0, AnastasiaLineCategory.TheFinalLine,       "solidification_final",
                    "...I can feel the ground."),
                Line(103, 0, AnastasiaLineCategory.PersonalReflection, "post_solidification",
                    "Ten seconds. That's more than most people get in a lifetime."),

                // ══════════════════════════════════════════════
                // Easter Egg Lines (Golden Motes) — 3 lines
                // ══════════════════════════════════════════════
                Line(104, 1, AnastasiaLineCategory.EasterEgg, "mote_0",
                    "You found it. A piece of me I didn't know was missing."),
                Line(105, 6, AnastasiaLineCategory.EasterEgg, "mote_5",
                    "That shelf. That exact shelf. That's where my record is stored. My... biography in light."),
                Line(106, 11, AnastasiaLineCategory.EasterEgg, "mote_10",
                    "...this was my room. Before. The mote is sitting right where my bed used to be."),

                // ══════════════════════════════════════════════
                // Reserved (DLC / Live-Ops) — 5 lines
                // ══════════════════════════════════════════════
                Line(107, 0, AnastasiaLineCategory.LoreWhisper,       "reserved_dlc_01", ""),
                Line(108, 0, AnastasiaLineCategory.LoreWhisper,       "reserved_dlc_02", ""),
                Line(109, 0, AnastasiaLineCategory.LoreWhisper,       "reserved_dlc_03", ""),
                Line(110, 0, AnastasiaLineCategory.LoreWhisper,       "reserved_dlc_04", ""),
                Line(111, 0, AnastasiaLineCategory.LoreWhisper,       "reserved_dlc_05", ""),
            };
        }

        static AnastasiaLine Line(int id, int moon, AnastasiaLineCategory cat, string context, string text)
        {
            return new AnastasiaLine
            {
                id = id,
                moon = moon,
                category = cat,
                triggerContext = context,
                text = text
            };
        }
    }
}
