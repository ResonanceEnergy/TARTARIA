# TARTARIA WORLD OF WONDER — Localization Strategy
## Multi-Language Pipeline, Cultural Adaptation & Asset Management

---

> *"The Tartarian Empire was planetary. Its story should be told in every language that survived the Reset."*

**Cross-References:**
- [05_CHARACTERS_DIALOGUE.md](05_CHARACTERS_DIALOGUE.md) — Dialogue design rules (2 sentences, no player speech)
- [22_DIALOGUE_BRANCHING.md](22_DIALOGUE_BRANCHING.md) — ~4,000 dialogue nodes, JSON format
- [appendices/C_AUDIO_DESIGN.md](appendices/C_AUDIO_DESIGN.md) — Voice acting, 432 Hz audio spec
- [01_LORE_BIBLE.md](01_LORE_BIBLE.md) — Old Tartarian conlang, 3-6-9 numerology
- [08_MONETIZATION.md](08_MONETIZATION.md) — Regional pricing, App Store Optimization
- [10_ROADMAP.md](10_ROADMAP.md) — Phased launch timeline

---

## Table of Contents

1. [Localization Philosophy](#1-localization-philosophy)
2. [Language Tiers & Market Priority](#2-language-tiers--market-priority)
3. [Text Pipeline](#3-text-pipeline)
4. [Cultural Adaptation Guidelines](#4-cultural-adaptation-guidelines)
5. [Old Tartarian Conlang Handling](#5-old-tartarian-conlang-handling)
6. [Voice-Over Strategy](#6-voice-over-strategy)
7. [UI & Font Requirements](#7-ui--font-requirements)
8. [Asset Bundle Architecture](#8-asset-bundle-architecture)
9. [Regional Content Considerations](#9-regional-content-considerations)
10. [Budget & Timeline](#10-budget--timeline)
11. [QA & Validation](#11-qa--validation)
12. [Post-Launch Language Expansion](#12-post-launch-language-expansion)

---

## 1. Localization Philosophy

### Core Rules

| Rule | Rationale |
|---|---|
| **Localize meaning, not words** | Milo's humor must be funny in Japanese — translate the joke, not the literal words |
| **Silent protagonist = localization advantage** | No player voice acting needed. NPC dialogue only. |
| **Old Tartarian stays universal** | The conlang is the same in all languages — only the *descriptions/subtitles* translate |
| **2-sentence mobile limit survives translation** | German expands ~30% from English. Budget 2 sentences at German length. |
| **Cultural sensitivity without censorship** | Adapt visuals/references that don't translate, but never cut lore depth |
| **Ship English + 3 Tier 1 languages at launch** | Remaining languages in live-ops updates |

### Why Localization Matters for Tartaria

- **iOS global market:** 60% of App Store revenue comes from non-English markets
- **Tartaria's real-world fan base:** r/tartaria, r/culturallayer, alt-history communities span 30+ countries
- **Genre precedent:** Genshin Impact's global success required launch-day Chinese, Japanese, Korean, English
- **Apple featuring criteria:** Multi-language support significantly increases App Store featuring probability

---

## 2. Language Tiers & Market Priority

### Tier 1: Launch Languages (Global Launch — Month 10)

| Language | Code | Market | Revenue Potential | Text Expansion | Complexity |
|---|---|---|---|---|---|
| **English (US/UK)** | en | Base | $800k+ | — | Base |
| **Japanese** | ja | Japan | $400k+ | -10% shorter | Honorifics, 3 writing systems |
| **Simplified Chinese** | zh-Hans | China (mainland) | $350k+ | -30% shorter | CJK fonts, regulatory review |
| **Korean** | ko | South Korea | $200k+ | -10% shorter | Honorific levels, spacing rules |

### Tier 2: Soft-Launch / Month 13 (Live-Ops Phase 1)

| Language | Code | Market | Revenue Potential | Text Expansion |
|---|---|---|---|---|
| **Traditional Chinese** | zh-Hant | Taiwan, HK, Macau | $150k+ | -30% shorter |
| **German** | de | DACH region | $120k+ | +30% longer |
| **French** | fr | France, Quebec, Africa | $100k+ | +20% longer |
| **Spanish (LatAm)** | es-419 | Latin America | $100k+ | +15% longer |

### Tier 3: Year 2 Expansion

| Language | Code | Market | Text Expansion |
|---|---|---|---|
| **Portuguese (BR)** | pt-BR | Brazil | +15% longer |
| **Russian** | ru | Russia, CIS | +15% longer |
| **Thai** | th | Thailand | Variable |
| **Turkish** | tr | Turkey | +10% longer |
| **Indonesian** | id | Indonesia | +5% longer |

---

## 3. Text Pipeline

### 3.1 Source Text Management

All translatable text passes through a centralized string table system:

```
Localization/
├── en/
│   ├── dialogue_moon01.json
│   ├── dialogue_moon02.json
│   ├── ...
│   ├── dialogue_moon13.json
│   ├── ui_strings.json
│   ├── quest_descriptions.json
│   ├── item_names.json
│   ├── tutorial_text.json
│   ├── achievement_text.json
│   └── lore_codex.json
├── ja/
│   └── (mirrored structure)
├── zh-Hans/
│   └── (mirrored structure)
└── ko/
    └── (mirrored structure)
```

### 3.2 String Key Convention

```
{category}.{context}.{id}

Examples:
dialogue.milo.moon01_greeting     = "You can hear it, can't you? The frequency beneath the mud."
ui.hud.resonance_score_label      = "Resonance Score"
quest.M1-MS01.title               = "The First Note Beneath the Mud"
item.material.copper_ingot        = "Copper Ingot"
lore.prophecy_stone.echohaven_01  = "When the last spire sings..."
achievement.restore_first_dome    = "First Light"
```

### 3.3 Translation Workflow

```
┌──────────────┐    ┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│  Dev writes   │    │  Export to    │    │  Translator   │    │  Import &    │
│  en strings   │───→│  TMS (Crowdin │───→│  translates   │───→│  build test  │
│  with context │    │  or Lokalise) │    │  with context │    │  in Unity    │
└──────────────┘    └──────────────┘    └──────────────┘    └──────────────┘
        │                                                           │
        │                    ┌──────────────┐                       │
        └────────────────────│  LQA review   │◄──────────────────────┘
                             │  (in-context) │
                             └──────────────┘
```

**TMS (Translation Management System):** Crowdin (recommended) or Lokalise
- Real-time sync with Git repository
- Context screenshots attached to every string
- Translation memory for consistency across Moons
- Glossary enforcement (see §5 for Tartarian terms)

### 3.4 String Expansion Buffer

UI elements must be designed with expansion headroom:

| UI Element | English Max | Buffer | Design Max |
|---|---|---|---|
| Dialogue bubble | 60 chars / 2 lines | +40% | 84 chars / 3 lines |
| Button label | 12 chars | +50% | 18 chars |
| Quest title | 40 chars | +35% | 54 chars |
| Item name | 25 chars | +40% | 35 chars |
| Tooltip | 80 chars | +30% | 104 chars |
| Achievement title | 30 chars | +40% | 42 chars |

---

## 4. Cultural Adaptation Guidelines

### 4.1 Universal Adaptations

| Element | English | Adaptation Notes |
|---|---|---|
| **Number formatting** | 1,000,000 | JA: 100万, DE: 1.000.000, ZH: 100万 |
| **Date formatting** | March 24, 2026 | JA: 2026年3月24日, DE: 24. März 2026 |
| **Color symbolism** | Gold = power/wonder | JA: Gold OK; ZH: Red = luck, White = death — review |
| **Currency display** | $4.99 | Regional App Store prices (¥800, €5.49, ₩6,500) |
| **Humor** | Milo's British sarcasm | JA: Convert to tsukkomi-style; KO: Adapt to aegyo-style |
| **Directional metaphors** | "Move forward" | RTL languages: directional UI flip (future Arabic/Hebrew) |

### 4.2 Milo's Humor Translation Guide

Milo is the most humor-dependent character. Each language needs a **humor consultant** (not just a translator):

| English Line | Translation Strategy |
|---|---|
| "Everything's for sale, spark." | JA: Keep merchant archetype (商人キャラ), adjust slang |
| "Maybe I'm a builder, not a seller." | Universal — direct translation works |
| "Giant problems require giant solutions." | Size pun — find equivalent pun per language |
| "I didn't run. I strategically relocated." | Cowardice humor — adapt to cultural comedy tropes |

### 4.3 Religious & Historical Sensitivity

| Element | Concern | Mitigation |
|---|---|---|
| **Sacred geometry** | Religious association (Islamic patterns, Christian crosses) | Frame as universal mathematical harmony, not religious |
| **Alternative history narrative** | Real historical conspiracy overlap in some markets | Frame as fantasy/mythology, not claims of literal truth |
| **Giants in world mythology** | JA: Daidarabotchi; ZH: Pangu — opportunity! | Link to local giant mythology where beneficial |
| **Mud Flood theory** | Western alt-history — less known in Asia | Add regional mythology equivalents as bonus lore |
| **13-Moon calendar** | Lunar significance varies by culture | Emphasize cosmic/mathematical beauty over spiritual claims |

---

## 5. Old Tartarian Conlang Handling

### 5.1 Conlang Stays Universal

Old Tartarian is a **constructed language** based on 3-6-9 numerological patterns. It appears in:
- Prophecy stone inscriptions
- Building glyphs
- Echo NPC fragment speech
- Cosmic event narrations

**Translation Rule:** The conlang itself is NEVER translated. Only the subtitles/descriptions are:

```
[Old Tartarian glyph appears on screen]
Subtitle (EN): "When the last spire sings, the grid awakens."
Subtitle (JA): 「最後の尖塔が歌うとき、グリッドは目覚める。」
Subtitle (ZH): "当最后的尖塔歌唱时，网格将苏醒。"
```

### 5.2 Tartarian Glossary (Non-Translatable Terms)

These terms remain in English across all languages (with phonetic guide in CJK):

| Term | Pronunciation Guide (JA) | Usage |
|---|---|---|
| Aether | エーテル (ēteru) | Core resource — always romanized |
| Resonance Score (RS) | レゾナンススコア (rezonansu sukoa) | Abbreviated RS in all languages |
| Ley Line | レイライン (rei rain) | Geographic term |
| Mud Flood | マッドフラッド (maddo furaddo) | Historical event name |
| Golden Age | ゴールデンエイジ (gōruden eiji) | Era name |
| Tartarian | タルタリアン (tarutarian) | Civilization name |
| Elara | エラーラ (erāra) | Character name |
| Milo | マイロ (mairo) | Character name |

### 5.3 Translatable Game Terms

These terms ARE translated — they are game mechanics, not lore:

| English | JA | ZH | KO |
|---|---|---|---|
| Dome | ドーム | 圆顶 | 돔 |
| Spire | 尖塔 | 尖塔 | 첨탑 |
| Star Fort | 星形要塞 | 星堡 | 성형 요새 |
| Companion | 仲間 | 同伴 | 동료 |
| Quest | クエスト | 任务 | 퀘스트 |
| Skill Tree | スキルツリー | 技能树 | 스킬 트리 |

---

## 6. Voice-Over Strategy

### 6.1 Phase 1 (Launch): Text Only

- All dialogue is text-based with character portraits
- No voice acting — consistent with mobile-first budget
- Companion "voice" conveyed through text style and portrait expressions

### 6.2 Phase 2 (Post-Launch): Selective VO

Priority VO if funded:
1. **Moon 13 climax** — 10 minutes of voiced companion lines (highest emotional impact)
2. **Milo's key moments** — 30 signature lines that define his arc
3. **Anastasia's 5 spoken words** — "My name is Anastasia Nikolaevna." (if she becomes fully solid)

### 6.3 VO Localization (If Implemented)

| Language | VO Strategy | Estimated Cost |
|---|---|---|
| English | Full selected lines | $15k–$25k |
| Japanese | Full selected lines (mandatory for JP market) | $20k–$35k |
| Chinese | Mandarin selected lines | $10k–$20k |
| Korean | Selected lines | $10k–$15k |
| Other | Subtitles only | $0 |

### 6.4 Audio Post-Processing

All VO must be post-processed to match the 432 Hz tuning standard:
- Record at standard 440 Hz
- Pitch-shift to 432 Hz in post-production
- Apply room reverb matching zone acoustics (cathedral, open air, underground)

---

## 7. UI & Font Requirements

### 7.1 Font Stack

| Script | Font | Fallback | Notes |
|---|---|---|---|
| **Latin** (EN, DE, FR, ES, PT) | Custom Tartarian display font | Noto Sans | Custom font for headers, system font for body |
| **CJK** (JA, ZH, KO) | Noto Sans CJK | System default | 15,000+ glyphs; bundle size impact ~8 MB |
| **Thai** | Noto Sans Thai | System default | Tone marks above/below baseline |
| **Cyrillic** (RU) | Noto Sans | System default | Subset: ~400 glyphs |

### 7.2 Text Rendering Rules

| Rule | Specification |
|---|---|
| Min font size | 11pt (accessibility minimum for mobile) |
| Line height | 1.4× font size (CJK: 1.6×) |
| Text wrapping | Word-wrap for Latin; character-wrap for CJK |
| Bidirectional | Future-proof layout with RTL containers (Arabic/Hebrew) |
| Dynamic sizing | TextMeshPro auto-size within fixed containers |
| Outline/Shadow | 1px outline on all in-world text for readability |

### 7.3 CJK-Specific Layout

| Element | Latin Layout | CJK Adaptation |
|---|---|---|
| Dialogue bubble | Horizontal 2-line | Horizontal 3-line (shorter chars, more per line) |
| Menu buttons | Single line | May need 2 lines (compound words) |
| Item descriptions | Paragraph | Shorter — CJK is more information-dense |
| Number display | 1,234,567 | JA/ZH: 123万4567 |

---

## 8. Asset Bundle Architecture

### 8.1 Language Packs as Addressable Groups

```
Addressables/
├── Core/          (universal — art, audio, code)
├── Lang_en/       (~2 MB — base strings)
├── Lang_ja/       (~3 MB — CJK strings + font subset)
├── Lang_zh-Hans/  (~3 MB)
├── Lang_ko/       (~3 MB)
├── Lang_de/       (~2.5 MB)
├── Lang_fr/       (~2.5 MB)
├── Lang_es-419/   (~2.5 MB)
└── VO_en/         (~50 MB — if VO implemented)
```

### 8.2 Download Strategy

| Scenario | Behavior |
|---|---|
| Fresh install | Download Core + device language pack (~155 MB total) |
| Language switch | Download new language pack on demand (~3 MB) |
| VO packs | Optional download prompt in settings |
| Update | Delta patches per language pack (typically <500 KB) |

---

## 9. Regional Content Considerations

### 9.1 China (zh-Hans) — Regulatory Compliance

| Requirement | Tartaria Impact | Mitigation |
|---|---|---|
| No skeletons/gore | N/A — Tartaria has no gore | Already compliant |
| No gambling mechanics | N/A — no loot boxes | Already compliant |
| Real-name registration | Backend requirement | Firebase auth + real-name API |
| Playtime limits (minors) | Anti-addiction system required | Implement playtime tracker + popup |
| Map of China | Any world map must show correct borders | Use fantasy map — no real-world overlay |
| "Conspiracy theory" filter | Alt-history framing | Frame as pure fantasy mythology |

### 9.2 Japan — Cultural Opportunities

| Opportunity | Implementation |
|---|---|
| Giant mythology (Daidarabotchi) | Add JP-exclusive lore fragment in Giant Mode |
| Sacred geometry (Shinto shrine proportions) | Include shrine archetype in building system |
| Seasonal events (Obon, Tanabata) | Tie live-ops events to Japanese festivals |
| Gacha expectation management | Clearly communicate "no gacha" as a feature |

### 9.3 Korea — Market Specifics

| Factor | Approach |
|---|---|
| Rating board (GRAC) | Apply for "All Ages" — content supports it |
| Real-name auth for minors | Integrate I-PIN verification |
| Competitive culture | Emphasize challenge modes, leaderboards |
| Social sharing | KakaoTalk sharing integration |

---

## 10. Budget & Timeline

### 10.1 Translation Costs (Estimated)

| Item | Per-Word Rate | Total Words | Per Language | 4 Tier 1 Languages |
|---|---|---|---|---|
| **Dialogue (4,000 nodes)** | $0.12–$0.18 | ~120,000 | $14,400–$21,600 | $43,200–$64,800 |
| **UI strings** | $0.10–$0.15 | ~8,000 | $800–$1,200 | $2,400–$3,600 |
| **Quest descriptions** | $0.12–$0.18 | ~25,000 | $3,000–$4,500 | $9,000–$13,500 |
| **Lore codex** | $0.15–$0.20 | ~30,000 | $4,500–$6,000 | $13,500–$18,000 |
| **Items & achievements** | $0.10–$0.15 | ~5,000 | $500–$750 | $1,500–$2,250 |
| **Total** | | ~188,000 words | $23,200–$34,050 | **$69,600–$102,150** |

### 10.2 Additional Costs

| Item | Cost |
|---|---|
| TMS License (Crowdin/Lokalise) | $5,000/year |
| CJK font licensing | $2,000–$5,000 one-time |
| LQA (linguistic QA) per language | $3,000–$5,000 |
| Humor consultant (JA, ZH) | $2,000–$3,000 per language |
| **Total Localization Budget (Tier 1)** | **$85,000–$125,000** |

### 10.3 Timeline Integration

| Phase | Localization Activity |
|---|---|
| Month 1–3 (Foundation) | Set up TMS, create glossary, establish string key conventions |
| Month 4–6 (MVP) | Translate MVP strings (Moon 1–3); LQA round 1 |
| Month 7–9 (Soft Launch) | English + JA soft launch; ZH/KO translation in progress |
| Month 10 (Global Launch) | All Tier 1 languages live |
| Month 13+ (Live-Ops) | Tier 2 languages added; ongoing translation for DLC |

---

## 11. QA & Validation

### 11.1 LQA (Linguistic Quality Assurance)

Each language undergoes 3 LQA passes:

| Pass | Focus | Method |
|---|---|---|
| **1. Textual** | Grammar, spelling, consistency, glossary adherence | Spreadsheet review |
| **2. Contextual** | Strings match in-game context (button fits, tooltip makes sense) | In-game screenshot review |
| **3. Functional** | Text renders correctly, no overflow, no encoding errors | Device playthrough per language |

### 11.2 Automated Checks

| Check | Tool | Trigger |
|---|---|---|
| Missing translations | TMS completion % | Pre-build CI/CD gate |
| String overflow | Unity TextMeshPro bounds check | Build-time |
| Encoding errors | UTF-8 validation script | Commit hook |
| Glossary violations | Crowdin QA check | Translation submission |
| Placeholder integrity | Regex: `{0}`, `{player_name}` etc. | Translation submission |

### 11.3 Community Translation Support

For Tier 3 languages, consider:
- Community translation program (with editorial review)
- Credit translators in-game
- Regional beta groups for cultural sensitivity feedback

---

## 12. Post-Launch Language Expansion

### 12.1 Language Addition Workflow

Each new language follows this 6-week pipeline:

```
Week 1:   Translator onboarding + glossary sync
Week 2-3: Translation (TMS-assisted, translation memory)
Week 4:   LQA Pass 1 (textual) + LQA Pass 2 (contextual)
Week 5:   Integration build + LQA Pass 3 (functional)
Week 6:   Addressable asset bundle build + deployment
```

### 12.2 Live-Ops Translation Cadence

| Content Type | Translation Turnaround |
|---|---|
| Seasonal event text | 2 weeks before event start |
| DLC expansion (per pack) | 4 weeks before release |
| Hotfix text changes | 48 hours |
| Community-reported errors | 1 week |

---

*The Tartarian truth belongs to everyone. Language should never be the mud that buries it.*

---

**Document Status:** DRAFT
**Author:** Nathan / Resonance Energy
**Last Updated:** March 24, 2026
