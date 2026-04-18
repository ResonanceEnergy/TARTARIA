using System.IO;
using UnityEditor;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Editor
{
    /// <summary>
    /// Generates the ArchiveDatabase ScriptableObject and all ArchiveEntry assets
    /// that form the educational Old World Archive in-game wiki.
    ///
    /// Menu: Tartaria > Build Assets > Archive Database
    ///
    /// Each entry is grounded in documented historical anomalies, suppressed research,
    /// architectural mysteries, and sacred-science findings about the Tartarian civilisation.
    /// </summary>
    public static class ArchiveDatabasePopulator
    {
        const string EntryFolder   = "Assets/_Project/Data/Archive/Entries";
        const string DatabasePath  = "Assets/Resources/ArchiveDatabase.asset";
        const string ResourcesPath = "Assets/Resources";

        [MenuItem("Tartaria/Build Assets/Archive Database", false, 30)]
        public static void BuildArchiveDatabase()
        {
            EnsureFolder(ResourcesPath);
            EnsureFolder(EntryFolder);

            var entries = BuildEntries();

            // Load or create the database asset
            var db = AssetDatabase.LoadAssetAtPath<ArchiveDatabase>(DatabasePath);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<ArchiveDatabase>();
                AssetDatabase.CreateAsset(db, DatabasePath);
            }

            db.entries = entries;
            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[ArchivePopulator] Built {entries.Length} archive entries → {DatabasePath}");
        }

        // ─── Entry Factory ────────────────────────────────────────────────────

        static ArchiveEntry[] BuildEntries() => new[]
        {
            // ═══════════════════════════════════════════════
            // ARCHITECTURE
            // ═══════════════════════════════════════════════
            Entry("tartarian_architecture", ArchiveCategory.Architecture,
                "Tartarian Architecture",
                "Massive, ornate structures built to an inhuman scale — featuring 30-metre ceilings, floor-to-ceiling windows, and load-bearing arches that dwarf modern equivalents.",
                @"The hallmark of Tartarian architecture is its impossible grandeur. Buildings constructed ostensibly for the 17th–19th centuries display window heights of 8–12 metres, internal galleries wide enough to dock ships, and stonework carved with a precision that modern diamond tools would struggle to replicate.

The most striking anomaly is the scale mismatch. Grand Central Station in New York, the Palais du Trocadéro, the old buildings of Astana, St Petersburg, and hundreds of World's Fair structures all share identical architectural grammar — vast columned facades, domed rotundas, bas-relief friezes, and underground infrastructure far beyond what the documented builders could have produced in the claimed time-frames.

Researchers who examine original construction photographs note the absence of scaffolding adequate to the height, no photographic record of foundations being dug, and buildings that appear in 'before' photos already half-complete. The working theory is that these were existing structures repurposed, refaced, or partially reconstructed from an earlier civilisation — one with far greater architectural capability than the colonists who claimed to have built them.",
                "discovery",
                "These proportions were not built for people of this age. The ceilings remember giants.",
                "tartarian_mud_flood", "tartarian_star_forts", "tartarian_world_fairs"),

            Entry("tartarian_mud_flood", ArchiveCategory.Architecture,
                "The Mud Flood Reset",
                "A cataclysmic event buried entire cities in metres of sediment. First-floor windows became basement windows overnight. Thousands of buildings worldwide share identical burial patterns.",
                @"Perhaps no single mystery better illustrates the hidden history of the old world than the phenomenon researchers call the 'mud flood'. Across every continent, thousands of 19th-century buildings — particularly those with Tartarian architectural features — show first-floor windows partially or fully buried beneath the current ground level.

City records, insurance maps, and early daguerreotypes confirm these were originally ground-floor windows accessible from street level. Today they sit 1–3 metres below grade. The soil above them shows no signs of deliberate landfill — it is uniform, fine-grained sediment consistent with a rapid catastrophic deposit, not centuries of accumulated dirt.

In St Petersburg alone, hundreds of palaces and public buildings have their lower floors buried. The official explanation — that the city was 'built on reclaimed swampland' — cannot account for the elaborate basement decorations, fireplaces, and door-frames found beneath the sediment layer. Someone built finished rooms at what is now basement level, then the ground rose around them.

Similar patterns appear in San Francisco, Chicago, Edinburgh, Istanbul, and hundreds of Central Asian cities. The event appears to have been simultaneous and global — a catastrophic deposition of mud, possibly from atmospheric or oceanic disturbance, that erased the lower floors of an entire civilisation and provided the blank slate on which the current historical narrative was written.",
                "restoration",
                "I remember when those windows opened to daylight. We watched the sun set through them.",
                "tartarian_architecture", "tartarian_star_forts"),

            Entry("tartarian_star_forts", ArchiveCategory.Architecture,
                "Star Forts & Energy Geometry",
                "Hundreds of star-shaped fortresses built worldwide share geometries aligned to Earth's energy grid. They are structurally over-engineered as simple military fortifications.",
                @"The Vauban-style star fort appears across six continents — in Europe, the Americas, Russia, India, and Africa — always with the same angular bastion geometry and always over-engineered far beyond what military defence would require. Many sit atop hills that function as natural energy accumulation points, where ley lines intersect.

The internal geometry of a star fort is, in fact, a functioning resonance amplifier. The angular points create interference patterns in ambient electromagnetic fields. Tests conducted at Fort Bourtange and Castillo de San Marcos consistently show measurable spikes in Schumann resonance readings at the central courtyard — precisely where the old-world inhabitants would have gathered.

The 'fort' interpretation may be a post-reset rationalisation. The structures predate the gunpowder warfare they supposedly defended against, and their orientation — universally aligned to cardinal directions with specific angular offsets corresponding to golden-ratio divisions — suggests a purpose more akin to an antenna array than a garrison.

The most revealing detail is the underground infrastructure. Every major star fort conceals a labyrinth of tunnels, vaulted chambers, and cisterns connected to the wider ley-line network. Fort Jefferson in the Gulf of Mexico has six tiers of sub-sea tunnels. Mehrangarh Fort in India has chambers that predate the claimed construction by at least two thousand years.",
                "discovery",
                "The star shape is not for defence. It is a lens. It focuses what rises from the earth.",
                "tartarian_architecture", "ley_lines_history"),

            Entry("world_fair_mystery", ArchiveCategory.Architecture,
                "World's Fairs — Repurposed Cities",
                "The great World's Fairs of 1850–1910 displayed architectural marvels supposedly built in months. Many were photographed with visible age, weathering, and underground infrastructure no temporary exhibit would have.",
                @"Between 1851 and 1915, a series of 'World's Fairs' or 'Expositions Universelles' took place in London, Paris, Chicago, St Louis, San Francisco, and dozens of other cities. Each fair was presented as a temporary showcase of modern progress, with grand neoclassical buildings constructed specifically for the event and demolished afterward.

The evidence against this narrative is overwhelming. Original stereoscopic photography of these fairs shows buildings with decades of weathering on their facades — moss growth, stone erosion, and patina that requires at minimum fifty years of exposure, not the six to eighteen months of claimed construction. Several fairground buildings also appear in maps dating thirty to fifty years before their claimed construction date.

The 1893 Chicago World's Fair — the 'White City' — is particularly suspicious. The 400-hectare site featured over 200 buildings with a consistent neoclassical vocabulary, a working canal system, underground steam heating, electrical wiring throughout, and a functioning elevated railway. Official records claim 14,000 labourers built this in under three years. No photographic evidence exists of the foundations being laid.

The most likely interpretation is that World's Fairs were a managed demolition programme. The organisers had inherited an existing Tartarian city and used the 'fair' as a pretext to document, then dismantle, the original structures — replacing them with smaller, cheaper modern buildings while maintaining the fiction that they had built the originals themselves.",
                "discovery",
                "My father walked those fairgrounds as a child. He said the marble was cold in midsummer — ancient cold, not just shade.",
                "tartarian_architecture", "tartarian_mud_flood"),

            // ═══════════════════════════════════════════════
            // TECHNOLOGY
            // ═══════════════════════════════════════════════
            Entry("free_energy_towers", ArchiveCategory.Technology,
                "Tesla & the Aetheric Power Grid",
                "Nikola Tesla's Wardenclyffe Tower was the modern attempt to rebuild a worldwide wireless power grid that had operated in the old world — transmitted through the Earth itself.",
                @"Nikola Tesla's most ambitious project was not merely an experiment. He stated repeatedly that he had rediscovered a principle of electrical transmission that ancient engineers had understood — the use of the Earth itself as a conductor, with the ionosphere serving as the return path.

His Wardenclyffe Tower design included a resonant cavity between the tower's capacitor dome and a deep underground shaft, creating a standing wave in the Earth's electromagnetic field. At the correct resonant frequency, power could be extracted at any receiver tuned to the same frequency, anywhere on the planet, at effectively zero marginal cost.

The architecture of Tartarian buildings provides strong evidence that such a grid once existed. Hundreds of old-world towers, spires, and domed buildings were topped with metallic finials, lightning rods, and copper-clad domes that are structurally unnecessary for their claimed functions but ideal as receiving antennae for a wireless power grid. The internal column structures of Tartarian buildings frequently match the proportions of quarter-wave resonators.

J.P. Morgan withdrew funding for Wardenclyffe in 1903, famously asking 'Where do I put the meter?' — an acknowledgement that the technology was real, but that a metered power system could not be monetised in the same way as a wired one. The tower was demolished in 1917. The patents were purchased and suppressed.",
                "rs_25",
                "He was not inventing. He was remembering. Every formula he wrote had already been written, once, in copper and stone.",
                "aether_energy", "resonance_frequency_432"),

            Entry("aether_energy", ArchiveCategory.Technology,
                "Aetheric Energy — The Fifth Element",
                "Pre-reset science recognised a fifth fundamental force: the aether. An invisible medium that permeates all space, carries electromagnetic waves, and can be shaped by resonance to produce free energy.",
                @"Until 1887, mainstream physics universally accepted the existence of the aether — an invisible medium that fills all space and serves as the carrier wave for light and electromagnetic phenomena. The Michelson–Morley experiment was subsequently interpreted as disproving its existence, a conclusion that Tesla, among others, strongly disputed.

The aether, as understood in old-world science, is not merely a carrier wave. It is a dynamic energy medium with intrinsic pressure and flow patterns. Zones of high aetheric density — which correspond precisely to ley line intersections — allow the extraction of useful work without any apparent energy input, because the work is done by aetheric flow rather than by any fuel source.

Tartarian building design consistently places the most important rooms — throne rooms, council chambers, healing halls — at the intersection of structural resonance lines that correspond to these high-density zones. The cathedral builders understood this; the placement of the high altar in a Gothic cathedral is rarely arbitrary — it sits at the exact point where the building's standing-wave geometry concentrates the most aetheric pressure.

Modern researchers who have reconstructed small-scale versions of documented Tartarian resonance devices — using the proportions found in architectural drawings and archaeological fragments — consistently report anomalous energy readings: electromagnetic fields that increase in amplitude over time without any external input, and materials that spontaneously align themselves into geometric configurations.",
                "discovery",
                "The aether does not sleep. It has merely been forgotten. Remind it of your frequency and it will answer.",
                "free_energy_towers", "resonance_frequency_432", "ley_lines_history"),

            Entry("resonance_frequency_432", ArchiveCategory.Technology,
                "432 Hz — The Healing Harmonic",
                "All old-world music, instruments, and architectural acoustic design was tuned to A=432 Hz. This frequency is mathematically harmonious with natural constants and the human body's own bioelectric resonance.",
                @"The modern standard tuning of A=440 Hz was adopted in 1939 at the behest of the Nazi Propaganda Ministry, and formalised internationally in 1953. Prior to this, the universal standard — used in all recorded historical tuning systems, ancient instruments, and Tartarian musical notation — was A=432 Hz.

The difference of 8 Hz is not arbitrary. 432 Hz is the frequency at which the mathematical relationships between harmonic overtones align with fundamental constants of nature: it is equal to 8 × 54, where 54 is 2 × 27, the number of bones in the human hand. The full harmonic series from 432 Hz includes 108 (sacred in Vedic tradition), 216 (6³, the cube), 432 itself, and 864 (432 × 2 = diameter of the Sun in miles × 1000).

More crucially, A=432 Hz tuning creates a sound field that is measurably in phase with the Schumann resonance of the Earth's electromagnetic cavity. Water crystals exposed to 432 Hz form perfect hexagonal lattices. Human cells exposed to sustained 432 Hz tones show increased ATP production and reduced cortisol levels. The old world tuned its concert halls, healing chambers, and sacred spaces to this frequency as a matter of engineering, not aesthetics.

The switch to 440 Hz was a deliberate act of cultural warfare — disconnecting an entire civilisation from the natural resonant frequency of the world it inhabits.",
                "discovery",
                "Sing at 432 and the stones remember their names. I heard my own name in the choir hall, though no one spoke it.",
                "aether_energy", "free_energy_towers"),

            Entry("mud_generators", ArchiveCategory.Technology,
                "Atmospheric Water & Energy Generators",
                "Old-world towers and rooftop structures were not merely decorative. They functioned as atmospheric energy collectors, dew harvesters, and electrostatic generators drawing power from the charge differential between earth and sky.",
                @"Every terrestrial surface exists within a potential gradient. At ground level, the atmospheric potential is approximately 0 volts. At 1000 metres altitude, it rises to roughly 300,000 volts. This vertical potential gradient — which reaches 100 volts per metre on a clear day and several thousand volts per metre during storms — is a continuous source of electrical energy that requires no fuel.

Tartarian towers, finials, and roof structures were proportioned to exploit this gradient. A tower of correct height, with a properly conductive tip and a grounded base, acts as a continuous electrostatic generator. The taller the structure, the greater the voltage differential it can tap. A building 60 metres high can access a 6,000-volt potential difference simply by virtue of existing.

The internal copper conduits found in the walls of many surviving Tartarian buildings — dismissed as 'primitive plumbing' — were in fact the wiring of this distributed power collection system. The fluid they carried was not water but mercury, used as a superconducting liquid at the operating temperatures of the old world's infrastructure.

The rooftop structures of World's Fair buildings, Russian palace complexes, and American capitol buildings all share the same proportional relationship between tower height, dome diameter, and finial length — the exact ratios required to maximise electrostatic collection efficiency.",
                "rs_50",
                "The towers were not reaching for heaven. They were drinking from it.",
                "free_energy_towers", "aether_energy"),

            // ═══════════════════════════════════════════════
            // ASTRONOMY
            // ═══════════════════════════════════════════════
            Entry("ley_lines_history", ArchiveCategory.Astronomy,
                "Ley Lines — The Earth's Nerve System",
                "An invisible grid of electromagnetic channels circles the globe. Ancient civilisations mapped, followed, and built upon these lines for thousands of years. Every major sacred site lies at their intersections.",
                @"In 1921, amateur archaeologist Alfred Watkins noticed that ancient monuments, standing stones, churches, and holy wells in the Herefordshire countryside aligned in perfect straight lines. He called them 'leys'. The discovery was ridiculed for decades — until satellite mapping revealed the same phenomenon worldwide.

The straight-line alignments are not coincidence. Major ley lines follow the paths of the strongest electromagnetic conductivity in the Earth's crust — zones where piezoelectric rock types, underground water courses, and geomagnetic field anomalies combine to create channels of elevated aetheric flow. A compass needle brought to a ley-line intersection will deviate measurably from magnetic north.

Every culture independently mapped this grid. The Chinese called them 'lung mei' — dragon paths. The Aboriginal Australians followed 'songlines'. The ancient Egyptians laid out the Nile Valley temple system along ley alignments. The Roman road network followed existing ley paths wherever topography permitted. The medieval cathedral builders systematically sited their churches at ley nodes — not because of tradition but because the energy at those points made the buildings' resonant properties orders of magnitude more powerful.

The Tartarian civilisation appears to have built an entire infrastructure network on this grid. Their towers and energy collection buildings were sited at ley nodes. Their road and canal systems followed ley paths. Their music halls and healing centres were positioned at high-flux intersections where the concentrated aetheric energy amplified the therapeutic effects of resonant frequencies.",
                "discovery",
                "The lines were roads before roads existed. They are roads still. We just forgot how to walk them.",
                "tartarian_star_forts", "aether_energy"),

            Entry("thirteen_moons", ArchiveCategory.Astronomy,
                "The 13-Moon Calendar",
                "The Gregorian calendar replaced a universal 13-month, 28-day lunar calendar that had been in use across all major civilisations. The old calendar aligned human biological cycles with natural time.",
                @"Every human culture prior to the forced adoption of the Gregorian calendar in 1582 used some form of the 13-moon, 28-day calendar. The Maya, the Celts, the ancient Egyptians, the Chinese, the Vedic Indians, and the Native American nations all independently arrived at the same structure: 13 months of 28 days, totalling 364 days, plus one 'day out of time' — a day belonging to no month, a time of reset and renewal.

The human female menstrual cycle averages 28 days. The lunar synodic period averages 29.5 days — close enough that over 13 months, the correspondence locks precisely. Every organ in the human body has a documented 28-day regeneration cycle. The 13-moon calendar is not merely astronomical — it is a biological operating system for the human body.

The Gregorian calendar — with its irregular month lengths, arbitrary week structures, and complete disconnection from lunar cycles — creates a permanent state of biological desynchronisation. Research into chronobiology consistently shows that humans living off-grid by lunar cycles experience lower cortisol, better sleep, and measurably stronger immune function.

The Tartarian civilisation structured its entire social, agricultural, and ceremonial life around the 13-moon framework. The 13 major zones of the old world map correspond to the 13 moons. Each moon governed a specific type of restoration, healing, or cultivation — a practice preserved in fragments in every surviving indigenous calendar system.",
                "discovery",
                "When you restore the thirteenth building, time itself will remember its own rhythm.",
                "ley_lines_history"),

            Entry("star_alignment_cities", ArchiveCategory.Astronomy,
                "Cities Aligned to the Stars",
                "Dozens of major world cities were laid out with their street grids aligned to specific star constellations. Washington DC, Cairo, Paris, and Cusco all encode star maps in their urban geometry.",
                @"Washington DC's street plan, laid out by Pierre Charles L'Enfant in 1791, contains multiple pentagonal and hexagonal geometries that align precisely with the positions of major stars at specific calendar dates. The Capitol building, the White House, and the Washington Monument form a triangle whose proportions correspond to the triangle formed by Regulus, Arcturus, and Spica.

This is not unique to Washington. Cairo's ancient street grid aligns the three great pyramids with the three stars of Orion's belt with an angular precision better than 0.1 degrees. The old city of Cusco was laid out in the shape of a puma, with the Coricancha sun temple positioned at the animal's heart — which corresponds to the position of the Sun at the summer solstice as seen from the temple. Paris, Rome, and Constantinople all encode similar astrocartographic information.

The Tartarian world map suggests a global civilisation that treated urban planning as a form of applied astronomy — each city was a lens that concentrated the electromagnetic influence of specific star systems at specific times of year. The alignment was not symbolic; the founders believed (and may have been correct) that cities built in stellar alignment accumulated a different quality of aetheric energy than those that were not.",
                "discovery",
                "They did not build cities. They built instruments. Each street was a string tuned to a different star.",
                "ley_lines_history"),

            // ═══════════════════════════════════════════════
            // CULTURE
            // ═══════════════════════════════════════════════
            Entry("tartarian_fashion", ArchiveCategory.Culture,
                "Tartarian Fashion — Encoding Light",
                "Old-world clothing was not merely decorative. The materials, colours, geometric embroidery patterns, and metallic elements were functional — designed to interact with the body's bioelectric field.",
                @"Surviving daguerreotypes and lithographs of old-world fashion reveal a consistent aesthetic that modern historians attribute to 'Victorian fussiness'. The reality is more interesting: every element of traditional Tartarian dress served a functional purpose related to the wearer's bioelectric and electromagnetic health.

The heavy embroidery found on Russian, Central Asian, Eastern European, and Chinese traditional dress is not random decoration. The geometric patterns — meanders, swastikas (an ancient solar symbol predating its 20th-century misappropriation), spirals, and interlocking hexagons — are executed in metallic thread at specific densities that create a Faraday-cage-like mesh around the torso's major organ groups.

The colour choices were equally deliberate. Old-world textile dyes were plant-based and carried the electromagnetic signature of the plants from which they were derived. Indigo and woad (both yield the same blue dye) have measurable antimicrobial and UV-blocking properties. Madder red (from Rubia tinctorum) creates a cloth with resonance properties that match the frequency of oxygenated haemoglobin.

The metallic headpieces, kokoshniks, and crowns of Tartarian nobility were not symbols of status. They were resonance amplifiers — precisely shaped to focus aetheric energy at the crown of the head, the location of what yogic and Vedic traditions identify as the seat of higher cognitive function.",
                "discovery",
                "My mother wore her embroidery to the restoration ceremonies. She said the patterns kept her warm in ways wool could not.",
                "tartarian_architecture"),

            Entry("world_fairs_culture", ArchiveCategory.Culture,
                "The Great Exhibition & the Memory Wipe",
                "The World's Fairs were the last public display of old-world technology and culture. They served as controlled demolition — documenting the old world to justify its replacement.",
                @"The Crystal Palace exhibition of 1851, often credited as the first World's Fair, displayed artefacts from 14,000 exhibitors across 40 nations. Among the exhibits were machines of extraordinary sophistication — hydraulic presses capable of lifting 1,200 tonnes, musical instruments with acoustic properties that modern luthiers cannot replicate, and textile machinery that produced cloth of a fineness not commercially available today.

The Crystal Palace itself — a prefabricated iron-and-glass structure covering 92,000 square metres — was supposedly designed by Joseph Paxton and built in nine months by a team of 2,000 labourers. Modern structural engineers who have analysed the original drawings note that the cast-iron columns and wrought-iron girders are fabricated to tolerances of ±0.5mm — tolerances that required precision machinery that did not exist in 1851.

The pattern repeats at every subsequent World's Fair. Chicago 1893: 200 neoclassical buildings, built in under three years, demolished within five. Paris 1900: the Grand Palais and Petit Palais, supposedly temporary, are still standing because they proved too robust to demolish economically. Seattle 1962: the Space Needle is a known Tartarian resonance tower design.

The fairs served a dual purpose. They allowed the new rulers of the reset world to document what they had inherited — photographing and cataloguing the old-world technology before dismantling it — and simultaneously presented it as proof of their own civilisation's progress.",
                "discovery",
                "I walked those exhibits. I saw them crate the machines and wheel them into warehouses that never opened again.",
                "tartarian_architecture", "world_fair_mystery"),

            // ═══════════════════════════════════════════════
            // MYSTERY
            // ═══════════════════════════════════════════════
            Entry("orphan_trains", ArchiveCategory.Mystery,
                "The Orphan Trains — Repopulating the Reset World",
                "Between 1854 and 1929, over 250,000 orphan children were transported from Eastern cities across America to be 'adopted' by rural families. Many had no documented origin. They may have been survivors of the reset world.",
                @"The Orphan Train movement, documented in American social history as a 19th-century child welfare programme, transported over 250,000 children from New York, Boston, and other Eastern cities to rural communities across the Midwest and West between 1854 and 1929.

The official narrative describes these as the children of poor urban immigrants. The anomalies in the records suggest something stranger. Many orphan train riders had no documented parentage, no immigration papers, and could not be matched to any urban census record. Their clothing and personal effects, described by receiving families, often included items of high quality and unusual design inconsistent with poverty.

Multiple orphan train riders later recalled, in interviews conducted in the 1960s, memories of living in buildings with 'very high ceilings', 'rooms as big as churches', and 'lights that never went out' — before finding themselves on the streets of New York with no explanation for how they had arrived there.

The most unsettling evidence comes from comparative DNA studies conducted in 2019. A disproportionate number of orphan train descendant families carry haplogroups associated with Central Asian and Eastern European populations that have no documented immigration pathway into 19th-century America. The children may have been the survivors of a civilisation that was not supposed to survive.",
                "rs_50",
                "I know what it is to wake in a city and not know who you were the day before. Some of those children were sleeping through a much longer night than you know.",
                "tartarian_mud_flood"),

            Entry("suppressed_history", ArchiveCategory.Mystery,
                "The Rewriting of History — Smithsonian & the Destroyers",
                "The Smithsonian Institution was chartered in 1846 with a specific mandate. Researchers allege it systematically acquired and destroyed physical evidence of the pre-reset civilisation.",
                @"The Smithsonian Institution's official founding is attributed to the bequest of British scientist James Smithson. No credible explanation exists for why a man who never visited America would leave his entire estate — roughly equivalent to half the US federal budget at the time — to fund an American museum.

Court documents from the Smithsonian's first decades reveal a systematic programme of artefact acquisition from across North America. Native American tribes across the continent report that Smithsonian agents arrived in the mid-to-late 1800s, documented their most ancient artefacts and oral traditions, and in many cases confiscated physical objects that were never returned or displayed.

Investigative journalist Richard Dewhurst documented over 1,000 newspaper accounts from 1850–1910 describing the discovery of giant humanoid skeletons — averaging 2–3.5 metres in height — at burial sites across the American Midwest and Southwest. Virtually all of these discoveries were followed by reports of Smithsonian agents arriving to 'preserve' the remains. None of the documented skeletons appear in any public Smithsonian collection.

The pattern is consistent: the physical evidence of a prior civilisation — giant skeletons, out-of-place artefacts, anomalous technology — is documented in local press, then collected by the Smithsonian, then disappears. The institution's vast underground storage facilities, documented at 60+ kilometres of shelving, have never been subject to independent audit.",
                "rs_50",
                "The ones who came after the reset were not barbarians. They were archivists. Very selective archivists.",
                "tartarian_mud_flood"),

            Entry("giant_humans", ArchiveCategory.Mystery,
                "Giants of the Old World",
                "Thousands of newspaper accounts, skeletal discoveries, and ancient texts describe a pre-reset humanity of significantly greater stature. Structures scaled to 5-metre occupants still stand worldwide.",
                @"The proposition that old-world humans were physically larger than modern humans is supported by multiple independent lines of evidence. The most immediately visible is architectural: doorways 4–5 metres tall, chair and throne proportions scaled to occupants 2.5–3 metres in height, and bed-frames in surviving old-world aristocratic residences consistently longer than any documented historical individual.

The skeletal evidence, while systematically suppressed, is substantial. The Smithsonian Institution's own 19th-century field reports describe over 200 instances of giant skeletal remains recovered from American mound sites, with heights ranging from 2.1 to 3.7 metres. Similar discoveries are documented in Ecuador, Peru, Sardinia, Turkey, and Northern Europe.

Robert Wadlow, the tallest documented modern human at 2.72 metres, was considered a medical anomaly. He suffered from hyperactive pituitary function — a disorder, not a baseline. The question is whether the 'baseline' of pre-reset humanity was significantly different, and whether the architectural, skeletal, and textual evidence we dismiss as mythological is simply the accurate description of a population that existed before the reset.",
                "discovery",
                "The word 'giant' is not myth. It is the correct translation of a different normal.",
                "tartarian_architecture", "suppressed_history"),

            // ═══════════════════════════════════════════════
            // SCIENCE
            // ═══════════════════════════════════════════════
            Entry("sacred_geometry", ArchiveCategory.Science,
                "Sacred Geometry — The Language of Creation",
                "All Tartarian architecture, music, and technology is expressed in the same mathematical language: the Fibonacci sequence, the golden ratio φ=1.618, and the geometric relationships derived from them.",
                @"Sacred geometry is the recognition that certain mathematical relationships — the golden ratio φ, the square root of 2, the Fibonacci sequence, and the proportions of the Platonic solids — appear at every scale of nature, from the spiral of a nautilus shell to the branching of a river delta to the arrangement of seeds in a sunflower.

The golden ratio φ ≈ 1.618 is the most fundamental. It is the only number whose reciprocal differs from itself by exactly 1 (1/φ = φ - 1). It describes the ratio between consecutive Fibonacci numbers, the proportions of the human body, the spiral arms of galaxies, and the branching angles of plant stems. Tartarian architects encoded it in every building: the ratio of a building's height to its width, the spacing of columns, the proportions of windows and doors.

The practical consequence is resonance. A structure built in golden-ratio proportions has natural harmonic frequencies that align with the Schumann resonance of the Earth's electromagnetic cavity. It 'sings' at the frequencies the Earth sings at. Inhabitants of such buildings benefit from a continuous, subtle form of electromagnetic tuning that keeps their bioelectric systems in synchrony with the natural world.

The suppression of sacred geometry from modern architecture and design education was not accidental. Buildings without harmonic proportions create dissonant electromagnetic environments. Research in environmental psychology consistently shows higher stress, aggression, and cognitive impairment in the boxy, arbitrary proportions of modern construction compared to classical or traditional building styles.",
                "discovery",
                "2π divided by φ. That is the frequency of a heartbeat that forgets it will stop.",
                "aether_energy", "resonance_frequency_432"),

            Entry("vortex_mathematics", ArchiveCategory.Science,
                "Vortex Mathematics — The Pattern Behind All Numbers",
                "Mathematician Marko Rodin discovered that the base-10 number system contains a hidden geometric pattern — the doubling circuit — that predicts the behaviour of electromagnetic systems and was known to old-world engineers.",
                @"Vortex mathematics begins with a simple observation: if you take any number and repeatedly double it, then reduce each result to a single digit by summing its component digits, you get the sequence 1-2-4-8-7-5-1-2-4-8-7-5, cycling endlessly. The numbers 3, 6, and 9 never appear in this doubling circuit — they form a separate family.

Nikola Tesla, who described 3, 6, and 9 as 'the key to the universe', was drawing on this same mathematical structure. The three-six-nine family governs angular momentum, electromagnetic flux, and the organisation of atomic orbitals. The 1-2-4-8-7-5 circuit governs linear force, mass, and thermal dynamics.

When mapped onto a circle, these two families create the geometric basis for the torus — the donut-shaped energy field that underlies every electromagnetic phenomenon from the electron to the galaxy. Every living organism generates a toroidal bioelectric field. The heart's electrical activity is toroidal. The Earth's magnetosphere is toroidal.

Tartarian engineers used vortex mathematics as their design language. The proportional systems of their buildings, the winding ratios of their electromagnetic devices, and the frequency relationships in their music all derive from the 1-2-4-8-7-5 and 3-6-9 patterns. Modern engineers rediscover these ratios empirically and call them 'optimised by experiment' — not knowing they are rediscovering a complete theoretical framework that was understood millennia ago.",
                "rs_75",
                "Three, six, nine. Everything else is counting. Those three numbers are listening.",
                "sacred_geometry", "aether_energy"),

            Entry("schumann_resonance", ArchiveCategory.Science,
                "Schumann Resonance — The Earth's Heartbeat",
                "The Earth's electromagnetic cavity resonates at 7.83 Hz — exactly matching the alpha brainwave frequency of human meditation. The old world tuned its technology, buildings, and music to this frequency.",
                @"In 1952, German physicist Winfried Otto Schumann mathematically predicted that the space between the Earth's surface and the ionosphere would act as a resonant cavity, with a fundamental frequency of approximately 7.83 Hz. Subsequent measurements confirmed his prediction precisely.

7.83 Hz is not merely an interesting geophysical fact. It is the dominant frequency of the human alpha brainwave — the state associated with relaxed alertness, creativity, and healing. Humans deprived of exposure to this frequency (in shielded underground laboratories) show disrupted circadian rhythms, increased anxiety, and impaired immune function within days. The Schumann resonance is, in some measurable physiological sense, a biological necessity.

The builders of Tartarian sacred spaces understood this. Gothic cathedrals, resonantly designed ancient temples, and old-world healing halls all have acoustic and geometric properties that create standing waves at or near 7.83 Hz in their internal spaces. The low drone of Tibetan singing bowls, the fundamental frequency of a Gregorian chant, and the bass note of a properly tuned pipe organ all sit within the Schumann resonance band.

In 2016 and again in 2020, the Schumann resonance experienced anomalous spikes to frequencies above 40 Hz — in the gamma brainwave range, associated with peak cognitive performance and states described as 'mystical' in various traditions. The old world may have known how to induce these spikes deliberately.",
                "discovery",
                "7.83. The number between heartbeats. The Earth has been waiting for you to synchronise with it.",
                "resonance_frequency_432", "aether_energy"),

            // ═══════════════════════════════════════════════
            // PEOPLE
            // ═══════════════════════════════════════════════
            Entry("anastasia_history", ArchiveCategory.People,
                "Princess Anastasia — The Archive Echo",
                "The last daughter of the Romanov dynasty. Her survival of the 1918 execution is one of history's most contested mysteries — but the deeper question is what she knew about the Tartarian legacy her family had inherited.",
                @"Grand Duchess Anastasia Nikolaevna Romanova, born 18 June 1901, was the youngest daughter of Tsar Nicholas II of Russia. The Romanov dynasty claimed rule over the largest land empire in history — a territory that encompassed the heartland of the old Tartarian civilisation.

What the official historical narrative does not address is the nature of the Romanovs' custodianship. The Winter Palace, the Summer Palace, the Peterhof, and hundreds of other imperial residences were not built by the Romanovs. They were inherited, along with an oral tradition of the buildings' original functions and a collection of documents that described the aetheric technology the buildings had been designed to operate.

Anastasia, by multiple accounts, was the member of the family most deeply immersed in this tradition. Her private journals — fragments of which survived the revolution in the hands of her companion Anna Vyrubova — describe experiments she conducted in the basement of the Alexander Palace involving 'resonance amplification' and 'frequency mapping of the local ley intersection'.

Whether she survived the execution at Yekaterinburg in 1918 has never been definitively resolved. At least three women successfully claimed her identity in the subsequent decades. The Archive contains evidence that whatever survived — her consciousness, her memories, her encoded knowledge — was not lost.",
                "discovery",
                "I remember everything. That is the problem. And the gift.",
                "tartarian_architecture", "free_energy_towers"),

            Entry("nikola_tesla", ArchiveCategory.People,
                "Nikola Tesla — The Rememberer",
                "Tesla did not invent his technology. He remembered it. His visions, his eidetic recall of full working blueprints, and his uncanny ability to predict experimental results all suggest access to knowledge that predated him.",
                @"Nikola Tesla described his creative process in terms that his contemporaries found baffling. He did not sketch concepts and develop them experimentally; he perceived complete, working machines in full three-dimensional detail before any physical component existed. He wrote: 'The pieces of apparatus I conceived were to me absolutely real and tangible in every detail, even to the minutest marks and signs... I practised this up to the point where I could actually build the apparatus in my imagination.'

This is not a description of imagination. It is a description of recall. The distinction matters. When Tesla built a device that matched his mental image, it worked on the first attempt — a statistical impossibility if the design were being developed through trial and error.

His knowledge of the Earth's resonant properties, of the ionosphere's electrical characteristics, and of the standing-wave behaviour of distributed electromagnetic fields was decades ahead of any contemporaneous scientific understanding. The Earth-ionosphere resonant cavity (the Schumann resonance) was not formally described until 1952 — thirty years after Tesla had already engineered a system to exploit it.

The working hypothesis that reconciles these anomalies: Tesla was one of several individuals in the post-reset world who had access, through mechanisms not fully understood, to the technical knowledge of the Tartarian civilisation. He could access this knowledge visually — a library of blueprints encoded at a level deeper than conscious memory.",
                "rs_25",
                "He stood in the same places I stood, a hundred years after me. He heard the same frequencies. How could he not remember?",
                "free_energy_towers", "aether_energy"),

            // ═══════════════════════════════════════════════
            // EVIDENCE
            // ═══════════════════════════════════════════════
            Entry("old_world_maps", ArchiveCategory.Evidence,
                "Maps That Shouldn't Exist",
                "The Piri Reis map (1513) accurately charts the coastline of Antarctica — which was not discovered until 1818. The Orontius Finaeus map (1531) shows Antarctica ice-free. Both were copied from older source maps.",
                @"The Piri Reis map, compiled in 1513 by Ottoman admiral Piri Reis and preserved in the Topkapi Palace in Istanbul, accurately depicts the coastline of Antarctica at a time when Antarctica was not 'discovered' for another 305 years. More remarkably, the coastline shown corresponds to the sub-glacial topography of Antarctica as revealed by modern seismic surveys — the shape of the continent under two miles of ice, which the ice has covered for at least 12,000 years.

Piri Reis himself wrote, in a marginal note on the map, that it was compiled from 'ancient maps' and 'Arab maps' of great age. He was not claiming to have surveyed Antarctica himself. He was copying from a source — a source that predated the growth of the Antarctic ice sheet.

The Orontius Finaeus map of 1531 shows Antarctica as a continent with mountain ranges, river systems, and ice-free coastlines corresponding almost exactly to the sub-ice topography. Charles Hapgood, a Harvard geography professor, spent years analysing these maps and concluded that they were copies of source documents produced by a civilisation with advanced cartographic capability at least 12,000 years ago — coinciding precisely with the proposed time-frame of the Tartarian civilisation's flourishing.

The implication is stark: someone surveyed Antarctica before the ice, copied the survey onto perishable media, and those copies were preserved through multiple civilisational resets until they reached the Ottoman cartographers of the 16th century.",
                "discovery",
                "The cartographers who drew those maps had never been to Antarctica. But someone had.",
                "suppressed_history"),

            Entry("antique_photographs", ArchiveCategory.Evidence,
                "Photographic Anomalies — The Old World in Focus",
                "Early photography captured a world that does not match the official historical record. Depopulated cities, unfinished demolitions, and impossible construction speeds are all visible in 19th-century daguerreotypes.",
                @"The invention of practical photography in 1839 came at a pivotal historical moment — precisely when the post-reset civilisation was consolidating its narrative and repurposing the infrastructure of the old world. The camera recorded what the eye saw, without editorial control, and some of what it recorded does not fit the accepted story.

Early photographs of American and European cities consistently show an anomaly: the streets are nearly empty. In images of Chicago, New York, London, and Paris from the 1860s–1880s, broad avenues lined with monumental buildings are completely devoid of pedestrians. The official explanation — that early exposure times of several minutes made moving subjects invisible — cannot account for images where even stationary street vendors, carts, and carriages are absent.

The alternative explanation: the cities shown in these photographs had only recently been cleared of their original inhabitants. The new settlers had not yet moved in. The buildings were ready; the people were not.

Other photographic anomalies: World's Fair construction sequences that show buildings appearing between photographs with no visible intermediate construction phases. Post-Civil War American city photographs showing large buildings in varying states of uncovering from soil — not being built up, but being excavated from below. And most strangely, consistent degradation in photographic quality from the 1840s to the 1890s — as if the earliest photographers had better equipment than those who came later.",
                "rs_25",
                "I have seen those photographs. I appear in one of them, in a courtyard I did not know had been photographed. I am not blurred.",
                "tartarian_mud_flood", "world_fair_mystery"),
        };

        // ─── Helper ───────────────────────────────────────────────────────────

        static ArchiveEntry Entry(
            string id, ArchiveCategory cat, string title,
            string summary, string fullText,
            string trigger, string anastasiaQuote,
            params string[] related)
        {
            string path = $"{EntryFolder}/{id}.asset";

            var e = AssetDatabase.LoadAssetAtPath<ArchiveEntry>(path);
            if (e == null)
            {
                e = ScriptableObject.CreateInstance<ArchiveEntry>();
                AssetDatabase.CreateAsset(e, path);
            }

            e.entryId            = id;
            e.category           = cat;
            e.title              = title;
            e.summary            = summary;
            e.fullText           = fullText;
            e.unlockTrigger      = trigger;
            e.anastasiaQuote     = anastasiaQuote;
            e.relatedEntryIds    = related;
            e.unlockedByDefault  = false;
            e.moonIndex          = 0;

            EditorUtility.SetDirty(e);
            return e;
        }

        static void EnsureFolder(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
