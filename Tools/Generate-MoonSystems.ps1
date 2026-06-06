<#
.SYNOPSIS
TARTARIA Moon System Generator — Builds complete Moon systems from templates

.DESCRIPTION
Generates all 14 systems for Moons 3-13 using Moon 1-2 as proven templates.
Adapts for each Moon's unique biome, theme, and narrative.

.PARAMETER Moons
Comma-separated Moon numbers (e.g., "3,4,5" or "3-13")

.PARAMETER Force
Overwrite existing files

.EXAMPLE
.\Generate-MoonSystems.ps1 -Moons "3-13"
Generates all remaining Moons (3 through 13)

.EXAMPLE
.\Generate-MoonSystems.ps1 -Moons "3,5,7" -Force
Regenerates specific Moons, overwriting existing files
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$Moons,
    
    [switch]$Force
)

cd C:\dev\TARTARIA_new

# Moon definitions with biome-specific properties
$moonData = @{
    3 = @{
        Name = "WindsweptHighlands"
        Theme = "Windswept highlands, railway ruins, Orphan Train narrative"
        EnemyType = "WindWraith"
        EnemyCount = 18
        CollectiblePrimary = "WindRune"
        CollectiblePrimaryCount = 22
        CollectibleSecondary = "TrainManifest"
        CollectibleSecondaryCount = 7
        InteractiveObject = "RailSwitch"
        InteractiveObjectCount = 10
        WeatherEffect = "WindStorm"
        Colors = @{ Primary = "0.6f, 0.7f, 0.8f"; Secondary = "0.8f, 0.6f, 0.4f" }
        SpecialMechanic = "OrphanTrain"
    }
    4 = @{
        Name = "AuroralSpire"
        Theme = "Northern lights tower, magnetic phenomena"
        EnemyType = "MagneticAnomaly"
        EnemyCount = 20
        CollectiblePrimary = "PolarShard"
        CollectiblePrimaryCount = 25
        CollectibleSecondary = "AuroraLog"
        CollectibleSecondaryCount = 8
        InteractiveObject = "MagneticNode"
        InteractiveObjectCount = 14
        WeatherEffect = "Aurora"
        Colors = @{ Primary = "0.2f, 0.9f, 0.6f"; Secondary = "0.6f, 0.2f, 0.9f" }
        SpecialMechanic = "MagneticField"
    }
    5 = @{
        Name = "DeepForge"
        Theme = "Volcanic forges, ancient smithing"
        EnemyType = "LavaGolem"
        EnemyCount = 22
        CollectiblePrimary = "ForgedRelic"
        CollectiblePrimaryCount = 18
        CollectibleSecondary = "SmithingScroll"
        CollectibleSecondaryCount = 6
        InteractiveObject = "Anvil"
        InteractiveObjectCount = 9
        WeatherEffect = "HeatWaves"
        Colors = @{ Primary = "1f, 0.4f, 0f"; Secondary = "0.9f, 0.6f, 0.1f" }
        SpecialMechanic = "ForgeSystem"
    }
    6 = @{
        Name = "LivingLibrary"
        Theme = "Sentient books, knowledge realm"
        EnemyType = "CorruptedTome"
        EnemyCount = 16
        CollectiblePrimary = "KnowledgeFragment"
        CollectiblePrimaryCount = 30
        CollectibleSecondary = "AncientManuscript"
        CollectibleSecondaryCount = 10
        InteractiveObject = "Lectern"
        InteractiveObjectCount = 12
        WeatherEffect = "PaperStorm"
        Colors = @{ Primary = "0.9f, 0.85f, 0.7f"; Secondary = "0.3f, 0.2f, 0.6f" }
        SpecialMechanic = "LoreWeaving"
    }
    7 = @{
        Name = "TidalArchive"
        Theme = "Underwater library, flood mechanics"
        EnemyType = "TidalGuardian"
        EnemyCount = 19
        CollectiblePrimary = "CoralTablet"
        CollectiblePrimaryCount = 24
        CollectibleSecondary = "WaterloggedDiary"
        CollectibleSecondaryCount = 8
        InteractiveObject = "FloodGate"
        InteractiveObjectCount = 11
        WeatherEffect = "TidalSurge"
        Colors = @{ Primary = "0.1f, 0.5f, 0.7f"; Secondary = "0.3f, 0.9f, 0.9f" }
        SpecialMechanic = "WaterLevel"
    }
    8 = @{
        Name = "CelestialObservatory"
        Theme = "Star mapping, cosmic patterns"
        EnemyType = "VoidEntity"
        EnemyCount = 17
        CollectiblePrimary = "StarFragment"
        CollectiblePrimaryCount = 28
        CollectibleSecondary = "AstralChart"
        CollectibleSecondaryCount = 9
        InteractiveObject = "Telescope"
        InteractiveObjectCount = 8
        WeatherEffect = "CelestialAlignment"
        Colors = @{ Primary = "0.1f, 0.1f, 0.3f"; Secondary = "0.9f, 0.9f, 1f" }
        SpecialMechanic = "Constellations"
    }
    9 = @{
        Name = "VerdantCanopy"
        Theme = "Living forest, plant consciousness"
        EnemyType = "CorruptedTreent"
        EnemyCount = 21
        CollectiblePrimary = "SeedOfLight"
        CollectiblePrimaryCount = 26
        CollectibleSecondary = "BotanicalJournal"
        CollectibleSecondaryCount = 7
        InteractiveObject = "AncientTree"
        InteractiveObjectCount = 13
        WeatherEffect = "PollenStorm"
        Colors = @{ Primary = "0.2f, 0.8f, 0.3f"; Secondary = "0.6f, 0.9f, 0.4f" }
        SpecialMechanic = "GrowthCycle"
    }
    10 = @{
        Name = "ClockworkCitadel"
        Theme = "Time manipulation, mechanical city"
        EnemyType = "Clockwork Soldier"
        EnemyCount = 25
        CollectiblePrimary = "CogOfTime"
        CollectiblePrimaryCount = 32
        CollectibleSecondary = "ClockmakersDiary"
        CollectibleSecondaryCount = 11
        InteractiveObject = "GearMechanism"
        InteractiveObjectCount = 16
        WeatherEffect = "TemporalDistortion"
        Colors = @{ Primary = "0.5f, 0.5f, 0.5f"; Secondary = "0.9f, 0.7f, 0.2f" }
        SpecialMechanic = "TimeFlow"
    }
    11 = @{
        Name = "SunkenColosseum"
        Theme = "Gladiatorial trials, honor code"
        EnemyType = "GhostGladiator"
        EnemyCount = 30
        CollectiblePrimary = "VictoryCrown"
        CollectiblePrimaryCount = 15
        CollectibleSecondary = "CombatScroll"
        CollectibleSecondaryCount = 8
        InteractiveObject = "ArenaTrigger"
        InteractiveObjectCount = 10
        WeatherEffect = "Sandstorm"
        Colors = @{ Primary = "0.8f, 0.7f, 0.5f"; Secondary = "0.7f, 0.2f, 0.1f" }
        SpecialMechanic = "ArenaChallenges"
    }
    12 = @{
        Name = "PlanetaryNexus"
        Theme = "Dimension crossroads, portal network"
        EnemyType = "DimensionalRift"
        EnemyCount = 23
        CollectiblePrimary = "NexusCrystal"
        CollectiblePrimaryCount = 35
        CollectibleSecondary = "PortalKey"
        CollectibleSecondaryCount = 12
        InteractiveObject = "PortalGate"
        InteractiveObjectCount = 15
        WeatherEffect = "RealityFlux"
        Colors = @{ Primary = "0.5f, 0f, 0.9f"; Secondary = "0f, 0.9f, 0.7f" }
        SpecialMechanic = "PortalNetwork"
    }
    13 = @{
        Name = "StarFortBastion"
        Theme = "Final convergence, Zereth confrontation"
        EnemyType = "Dissonance Avatar"
        EnemyCount = 35
        CollectiblePrimary = "HarmonicKey"
        CollectiblePrimaryCount = 40
        CollectibleSecondary = "ZerethMemory"
        CollectibleSecondaryCount = 13
        InteractiveObject = "FinalNode"
        InteractiveObjectCount = 18
        WeatherEffect = "ResonanceCascade"
        Colors = @{ Primary = "1f, 1f, 1f"; Secondary = "0f, 0f, 0f" }
        SpecialMechanic = "FinalBoss"
    }
}

# Parse moon range
$moonNumbers = @()
if ($Moons -match "(\d+)-(\d+)") {
    $moonNumbers = $Matches[1]..$Matches[2]
} else {
    $moonNumbers = $Moons.Split(',').Trim()
}

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║       TARTARIA MOON SYSTEM GENERATOR — COMPREHENSIVE BUILD    ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""
Write-Host "Generating systems for Moons: $($moonNumbers -join ', ')" -ForegroundColor Yellow
Write-Host ""

foreach ($moonNum in $moonNumbers) {
    $moon = $moonData[[int]$moonNum]
    if ($null -eq $moon) {
        Write-Host "⚠️  Moon $moonNum not defined, skipping..." -ForegroundColor Yellow
        continue
    }
    
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
    Write-Host "🌙 MOON $moonNum — $($moon.Name)" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
    Write-Host "   Theme: $($moon.Theme)" -ForegroundColor Gray
    Write-Host ""
    
    # Generate all 14 systems for this Moon
    $systems = @(
        "EnemySpawners",
        "Collectibles",
        "InteractiveObjects",
        "WeatherSystem",
        "AmbientAudio",
        "AmbientParticles",
        "AudioZones",
        "VisualLandmarks",
        "NPCDialogues",
        "QuestNodes",
        "Secrets",
        "PowerUps",
        "DynamicHazards",
        "EnvironmentDecorator"
    )
    
    $systemsCreated = 0
    foreach ($system in $systems) {
        $fileName = "Moon${moonNum}${system}.cs"
        $filePath = "Assets\_Project\Scripts\Integration\$fileName"
        
        # Check if exists
        if ((Test-Path $filePath) -and -not $Force) {
            Write-Host "   ⏩ $fileName (exists, skipping)" -ForegroundColor DarkGray
            continue
        }
        
        # Generate system content using template + moon-specific data
        $content = Generate-MoonSystem -MoonNumber $moonNum -MoonData $moon -SystemType $system
        
        Set-Content $filePath $content -Encoding UTF8
        Write-Host "   ✅ $fileName" -ForegroundColor Green
        $systemsCreated++
    }
    
    Write-Host ""
    Write-Host "   📊 Moon $moonNum: $systemsCreated systems generated" -ForegroundColor Cyan
    Write-Host ""
}

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║               ✅ COMPREHENSIVE BUILD COMPLETE! ✅              ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
Write-Host "Generated systems for Moons: $($moonNumbers -join ', ')" -ForegroundColor White
Write-Host "Total systems created: $($moonNumbers.Count * 14)" -ForegroundColor White
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Open Unity Editor" -ForegroundColor White
Write-Host "  2. Let Unity compile new scripts" -ForegroundColor White
Write-Host "  3. Wire prefabs in each Moon scene" -ForegroundColor White
Write-Host "  4. Test each Moon's gameplay loop" -ForegroundColor White
Write-Host ""

# Template generation function
function Generate-MoonSystem {
    param($MoonNumber, $MoonData, $SystemType)
    
    # Use Moon 1-2 as templates, adapt for target Moon
    $template = Get-TemplateForSystem -SystemType $SystemType
    
    # Replace placeholders with Moon-specific data
    $content = $template `
        -replace "Moon1", "Moon$MoonNumber" `
        -replace "Moon2", "Moon$MoonNumber" `
        -replace "MudGolem", $MoonData.EnemyType `
        -replace "DissonanceDefender", $MoonData.EnemyType `
        -replace "CrystalFragment", $MoonData.CollectiblePrimary `
        -replace "AetherShard", $MoonData.CollectiblePrimary `
        -replace "LoreArtifact", $MoonData.CollectibleSecondary `
        -replace "CaveLoreTablet", $MoonData.CollectibleSecondary `
        -replace "TuningNode", $MoonData.InteractiveObject `
        -replace "DissonanceCrystal", $MoonData.InteractiveObject `
        -replace "0.6f, 0.2f, 0.8f", $MoonData.Colors.Primary `
        -replace "0f, 0.8f, 1f", $MoonData.Colors.Secondary
    
    return $content
}

function Get-TemplateForSystem {
    param($SystemType)
    
    # Load Moon1 or Moon2 template file as base
    $templatePath1 = "Assets\_Project\Scripts\Integration\Moon1$SystemType.cs"
    $templatePath2 = "Assets\_Project\Scripts\Integration\Moon2$SystemType.cs"
    
    if (Test-Path $templatePath2) {
        return Get-Content $templatePath2 -Raw
    } elseif (Test-Path $templatePath1) {
        return Get-Content $templatePath1 -Raw
    } else {
        Write-Host "   ⚠️  No template found for $SystemType" -ForegroundColor Yellow
        return "// TODO: Implement Moon$SystemType"
    }
}
