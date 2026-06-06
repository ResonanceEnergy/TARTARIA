"""Procedurally generate the 17th-hour cathedral cinematic music stem.

60-second WAV — slow Eb pedal + rising harmonic series + cathedral bell at 30s
+ rising orchestral pad swell.

Mono, 44.1 kHz, 16-bit PCM. Drops at:
  Assets/_Project/Resources/Audio/Music/seventeenth_hour.wav

2026-06-05 — Autonomous Moon 1 content build.
"""
import math
import struct
import wave
import os
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
OUT = REPO / "Assets/_Project/Resources/Audio/Music/seventeenth_hour.wav"
OUT.parent.mkdir(parents=True, exist_ok=True)

SR = 44100
DUR = 60.0
N = int(SR * DUR)

# Frequencies — Eb minor cathedral set
EB2 = 77.78    # bass pedal
BB2 = 116.54
EB3 = 155.56
GB3 = 184.997  # minor 3rd
BB3 = 233.08
EB4 = 311.13
GB4 = 369.99
BELL = 528.0   # Celestial band tone

def env_ramp(t, dur, peak_t):
    """Slow attack ramp peaking near peak_t, gentle decay after."""
    if t < peak_t:
        return (t / peak_t) ** 1.4
    return max(0.0, 1.0 - (t - peak_t) / (dur - peak_t))

samples = []
for i in range(N):
    t = i / SR

    # ── Bass pedal Eb2 (full duration, slow swell)
    bass_env = env_ramp(t, DUR, 35.0) * 0.45
    bass = math.sin(2 * math.pi * EB2 * t) * bass_env

    # ── Mid pad (Bb3 + Eb4 minor triad), enters at 8s, swells through 35s
    pad_env = 0.0
    if t > 8.0:
        x = (t - 8.0) / 30.0
        pad_env = min(1.0, x) * 0.32
        if t > 48.0:
            pad_env *= max(0.0, 1.0 - (t - 48.0) / 12.0)
    pad = (math.sin(2 * math.pi * BB3 * t) + math.sin(2 * math.pi * EB4 * t * 1.001)) * pad_env * 0.5

    # ── Minor third Gb (haunting), enters at 18s
    minor_env = 0.0
    if t > 18.0:
        x = (t - 18.0) / 22.0
        minor_env = min(1.0, x) * 0.25
    minor = math.sin(2 * math.pi * GB3 * t) * minor_env

    # ── Celestial harmonic series (Gb4 + Eb4) enters at 24s
    celestial_env = 0.0
    if t > 24.0:
        x = (t - 24.0) / 16.0
        celestial_env = min(1.0, x) * 0.20
        if t > 50.0:
            celestial_env *= max(0.0, 1.0 - (t - 50.0) / 10.0)
    celestial = (math.sin(2 * math.pi * GB4 * t) + math.sin(2 * math.pi * EB4 * 1.5 * t)) * celestial_env * 0.5

    # ── Cathedral bell @ 30s (528 Hz, sharp attack + 8s decay)
    bell = 0.0
    if 30.0 <= t < 38.0:
        bell_t = t - 30.0
        decay = math.exp(-bell_t * 0.45)
        bell = math.sin(2 * math.pi * BELL * t) * decay * 0.32
        # Subtle 5th overtone
        bell += math.sin(2 * math.pi * BELL * 1.5 * t) * decay * 0.10

    # ── Sub-octave drone harmonics
    sub = math.sin(2 * math.pi * EB3 * t) * bass_env * 0.15

    sample = bass + pad + minor + celestial + bell + sub
    # Soft clip
    if sample > 0.95: sample = 0.95
    if sample < -0.95: sample = -0.95
    samples.append(int(sample * 32767.0))

# Write WAV
with wave.open(str(OUT), 'wb') as wf:
    wf.setnchannels(1)
    wf.setsampwidth(2)
    wf.setframerate(SR)
    wf.writeframes(b''.join(struct.pack('<h', s) for s in samples))

print(f"[gen_seventeenth_hour] Wrote {OUT} ({os.path.getsize(OUT)/1024:.1f} KB, {DUR}s @ {SR}Hz)")
