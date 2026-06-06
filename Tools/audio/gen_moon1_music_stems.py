#!/usr/bin/env python
"""Author all 4 Moon 1 music stems per docs/15 §16.11.

Generates:
  ambient_layer1.wav — ambient drone (always-on base layer)
  ambient_layer2.wav — exploration overlay (kicks in on RS gain)
  ambient_layer3.wav — orchestral pad (sustained string section)
  ambient_layer4.wav — triumphant brass (chord swells)

All 60s mono 44.1kHz 16-bit PCM, loopable (head/tail 100ms crossfade).
"""
import math
import struct
import wave
from pathlib import Path

SR = 44100
DURATION = 60.0  # seconds
OUT_DIR = Path(__file__).parent.parent.parent / "Assets" / "_Project" / "Resources" / "Audio" / "Music"


def write_wav(filename, samples):
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    path = OUT_DIR / filename
    with wave.open(str(path), 'wb') as wf:
        wf.setnchannels(1)
        wf.setsampwidth(2)  # 16-bit
        wf.setframerate(SR)
        # Pack in chunks to reduce memory churn on long buffers
        chunk = 8192
        for start in range(0, len(samples), chunk):
            block = samples[start:start + chunk]
            wf.writeframes(b''.join(
                struct.pack('<h', int(max(-32767, min(32767, s * 32767))))
                for s in block
            ))
    return path


def gen_layer1_ambient_drone():
    """Sustained low drone at A1 (55 Hz) + A2 (110 Hz) with very slow LFO sweep.

    Always-on base layer — quiet, atmospheric. Peak 0.4 to leave headroom for upper layers.
    """
    n = int(SR * DURATION)
    out = [0.0] * n
    # Two-voice fundamental drone with slight phase offset
    voices = [
        (55.00, 0.0),    # A1 — deep root
        (110.00, 0.15),  # A2 — octave above, subtle phase offset
        (55.27, 0.05),   # detuned A1 for chorus shimmer
    ]
    lfo_rate = 0.07  # Hz (~14s cycle) — very slow volume sweep
    two_pi = 2.0 * math.pi
    for freq, phase_offset in voices:
        for i in range(n):
            t = i / SR
            # Slow LFO volume swell + small high-harmonic content for warmth
            lfo = 0.75 + 0.25 * math.sin(two_pi * lfo_rate * t + phase_offset)
            envelope = min(1.0, t / 4.0) * min(1.0, (DURATION - t) / 4.0)
            voice_amp = 0.25 * lfo * envelope
            # Fundamental + soft 2nd harmonic for warmth (not pure sine — it would feel hollow)
            sample = math.sin(two_pi * freq * t + phase_offset)
            sample += 0.25 * math.sin(two_pi * freq * 2.0 * t + phase_offset)
            out[i] += voice_amp * sample
    peak = max(abs(s) for s in out)
    if peak > 0:
        scale = 0.4 / peak
        out = [s * scale for s in out]
    fade = int(SR * 0.1)
    for i in range(fade):
        f = i / fade
        out[i] *= f
        out[n - 1 - i] *= f
    return out


def gen_layer2_exploration_overlay():
    """Harp-like arpeggios on A minor (A, C, E, A oct = 110, 130.81, 164.81, 220 Hz).

    Quick attack, slow exponential decay per note. Notes cycle every 0.75s.
    Peak 0.7. Activates on RS gain — moderate presence so it's clearly heard layered on top.
    """
    n = int(SR * DURATION)
    out = [0.0] * n
    # A minor arpeggio: A2 C3 E3 A3 (cycle low->high then descend)
    arp_notes = [110.00, 130.81, 164.81, 220.00, 164.81, 130.81]
    note_dur = 0.75  # seconds per pluck
    two_pi = 2.0 * math.pi
    cycle = len(arp_notes) * note_dur
    for i in range(n):
        t = i / SR
        cycle_t = t % cycle
        note_idx = int(cycle_t / note_dur)
        note_t = cycle_t - (note_idx * note_dur)  # 0..note_dur into pluck
        freq = arp_notes[note_idx]
        # Quick attack (15ms), exponential decay tau ~ 0.35s
        if note_t < 0.015:
            env = note_t / 0.015
        else:
            env = math.exp(-(note_t - 0.015) / 0.35)
        # Harp character — fundamental + lightly weighted upper harmonics
        sample = math.sin(two_pi * freq * t)
        sample += 0.35 * math.sin(two_pi * freq * 2.0 * t)
        sample += 0.15 * math.sin(two_pi * freq * 3.0 * t)
        sample += 0.06 * math.sin(two_pi * freq * 4.0 * t)
        # Global phrase envelope to give the cycle some shape across the 60s
        phrase = 0.6 + 0.4 * math.sin(two_pi * (t / DURATION) * 2.0)
        full_env = min(1.0, t / 2.0) * min(1.0, (DURATION - t) / 2.0)
        out[i] = 0.4 * env * phrase * full_env * sample
    peak = max(abs(s) for s in out)
    if peak > 0:
        scale = 0.7 / peak
        out = [s * scale for s in out]
    fade = int(SR * 0.1)
    for i in range(fade):
        f = i / fade
        out[i] *= f
        out[n - 1 - i] *= f
    return out


def gen_layer3_orchestral_pad():
    """Sustained string section feel — 4-voice with slow tremolo + slight detune for chorus depth."""
    n = int(SR * DURATION)
    out = [0.0] * n
    voices = [
        (110.00, 0.0),    # A2 root
        (164.81, 0.2),    # E3 fifth (slightly delayed phase)
        (220.00, 0.4),    # A3 octave
        (110.5, 0.1),     # slight detune for chorus
    ]
    tremolo_rate = 4.5  # Hz
    two_pi = 2.0 * math.pi
    for freq, phase_offset in voices:
        for i in range(n):
            t = i / SR
            tremolo = 0.85 + 0.15 * math.sin(two_pi * tremolo_rate * t)
            # 3s attack/release envelope to give phrases shape and aid loop continuity
            envelope = min(1.0, t / 3.0) * min(1.0, (DURATION - t) / 3.0)
            voice_amp = 0.2 * tremolo * envelope
            out[i] += voice_amp * math.sin(two_pi * freq * t + phase_offset)
    # Normalize to 0.7 peak (leave headroom)
    peak = max(abs(s) for s in out)
    if peak > 0:
        scale = 0.7 / peak
        out = [s * scale for s in out]
    # Loop crossfade — gentle fade-in over first 100ms paired with fade-out over last 100ms
    fade = int(SR * 0.1)
    for i in range(fade):
        f = i / fade
        out[i] *= f
        out[n - 1 - i] *= f
    return out


def gen_layer4_triumphant_brass():
    """Major chord swell with brass harmonics + soft clipping. C major (C3 E3 G3 C4).

    Phrase: 4s attack, 4s sustain, 8s release. Cycle = 16s. ~3.75 cycles per 60s clip.
    """
    n = int(SR * DURATION)
    out = [0.0] * n
    chord = [130.81, 164.81, 195.99, 261.63]  # C3 E3 G3 C4
    cycle = 16.0
    harmonics = [(1, 1.0), (2, 0.6), (3, 0.4), (4, 0.25), (5, 0.15)]
    two_pi = 2.0 * math.pi
    for i in range(n):
        t = i / SR
        cycle_t = t % cycle
        if cycle_t < 4.0:
            env = cycle_t / 4.0
        elif cycle_t < 8.0:
            env = 1.0
        elif cycle_t < 16.0:
            env = (16.0 - cycle_t) / 8.0
        else:
            env = 0.0
        sample = 0.0
        for f in chord:
            for h, amp in harmonics:
                sample += amp * math.sin(two_pi * f * h * t)
        sample *= 0.1 * env  # mix down across 4 chord voices × 5 harmonics
        # Soft clip for brass character
        if sample > 0.7:
            sample = 0.7 + 0.3 * math.tanh((sample - 0.7) / 0.3)
        elif sample < -0.7:
            sample = -0.7 + 0.3 * math.tanh((sample + 0.7) / 0.3)
        out[i] = sample
    peak = max(abs(s) for s in out)
    if peak > 0:
        scale = 0.85 / peak
        out = [s * scale for s in out]
    # Loop crossfade
    fade = int(SR * 0.1)
    for i in range(fade):
        f = i / fade
        out[i] *= f
        out[n - 1 - i] *= f
    return out


if __name__ == '__main__':
    print("[gen_moon1_music_stems] Generating ambient_layer1.wav (ambient drone)...")
    samples1 = gen_layer1_ambient_drone()
    peak1 = max(abs(s) for s in samples1)
    p1 = write_wav("ambient_layer1.wav", samples1)
    print(f"  wrote {p1} ({p1.stat().st_size} bytes, peak={peak1:.4f})")

    print("[gen_moon1_music_stems] Generating ambient_layer2.wav (exploration overlay)...")
    samples2 = gen_layer2_exploration_overlay()
    peak2 = max(abs(s) for s in samples2)
    p2 = write_wav("ambient_layer2.wav", samples2)
    print(f"  wrote {p2} ({p2.stat().st_size} bytes, peak={peak2:.4f})")

    print("[gen_moon1_music_stems] Generating ambient_layer3.wav (orchestral pad)...")
    samples3 = gen_layer3_orchestral_pad()
    peak3 = max(abs(s) for s in samples3)
    p3 = write_wav("ambient_layer3.wav", samples3)
    print(f"  wrote {p3} ({p3.stat().st_size} bytes, peak={peak3:.4f})")

    print("[gen_moon1_music_stems] Generating ambient_layer4.wav (triumphant brass)...")
    samples4 = gen_layer4_triumphant_brass()
    peak4 = max(abs(s) for s in samples4)
    p4 = write_wav("ambient_layer4.wav", samples4)
    print(f"  wrote {p4} ({p4.stat().st_size} bytes, peak={peak4:.4f})")

    print("[gen_moon1_music_stems] DONE.")
