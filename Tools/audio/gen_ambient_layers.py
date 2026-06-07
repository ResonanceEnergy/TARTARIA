"""
R57 — Generate 4 ambient_layer*.wav files for AdaptiveMusicController's L1 baseline.
Each layer is 60s, 44.1 kHz, mono, 16-bit PCM. Designed to loop cleanly + crossfade.

Per docs/15 §13 + CLAUDE.md (Aether bands):
- Layer 1 Telluric (7.83 Hz Schumann transposed up to ~62 Hz audible drone)
- Layer 2 Harmonic (432 Hz reference + soft modulations)
- Layer 3 Celestial (528 Hz + harmonic 5ths)
- Layer 4 Combat (low rumble swell)

Output: Assets/_Project/Resources/Audio/Music/ambient_layer{1..4}.wav
"""
import math, struct, os, wave

SAMPLE_RATE = 44100
DURATION = 60.0
TOTAL_SAMPLES = int(SAMPLE_RATE * DURATION)

OUT_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)),
                      "..", "..", "Assets", "_Project", "Resources", "Audio", "Music")


def lerp(a, b, t):
    return a + (b - a) * t


def sine(freq, t, amp=1.0, phase=0.0):
    return amp * math.sin(2 * math.pi * freq * t + phase)


def soft_clip(x):
    """Soft compression so we never digital-clip even on overlap."""
    return math.tanh(x * 0.7)


def fade_envelope(i, total, fade_in_secs=2.0, fade_out_secs=2.0):
    """Linear fade in/out so seamless looping when AdaptiveMusic crossfades."""
    t = i / SAMPLE_RATE
    if t < fade_in_secs:
        return t / fade_in_secs
    if t > DURATION - fade_out_secs:
        return (DURATION - t) / fade_out_secs
    return 1.0


def write_wav(path, samples):
    """Write 16-bit PCM mono WAV."""
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SAMPLE_RATE)
        # Convert float -1..1 to int16
        bytes_data = bytearray()
        for s in samples:
            v = max(-1.0, min(1.0, s))
            i = int(v * 32767)
            bytes_data += struct.pack("<h", i)
        w.writeframes(bytes(bytes_data))


def gen_layer1_telluric():
    """Telluric — 7.83 Hz Schumann transposed up to ~62 Hz, with subbass + LFO."""
    samples = []
    for i in range(TOTAL_SAMPLES):
        t = i / SAMPLE_RATE
        # Base 62 Hz drone (Schumann * 8) + 31 Hz subbass octave
        v = 0.30 * sine(62.64, t)
        v += 0.15 * sine(31.32, t)
        # Slow LFO modulation (0.1 Hz) for organic feel
        lfo = 0.5 + 0.5 * sine(0.1, t)
        # Texture: high "earthly" 7th overtone
        v += 0.06 * sine(62.64 * 7, t) * lfo
        # Soft noise floor (low rumble)
        v += 0.02 * math.sin(t * 17.3) * math.cos(t * 23.7)
        v = soft_clip(v) * fade_envelope(i, TOTAL_SAMPLES, 4, 4) * 0.6
        samples.append(v)
    return samples


def gen_layer2_harmonic():
    """Harmonic — 432 Hz reference + soft fifth + slow tremolo."""
    samples = []
    for i in range(TOTAL_SAMPLES):
        t = i / SAMPLE_RATE
        # 432 Hz fundamental
        v = 0.18 * sine(432.0, t)
        # Perfect fifth (648 Hz)
        v += 0.10 * sine(648.0, t)
        # Octave below for warmth (216 Hz)
        v += 0.14 * sine(216.0, t)
        # Slow tremolo
        trem = 0.85 + 0.15 * sine(0.25, t)
        v *= trem
        # Subtle chorus shimmer (slight detune)
        v += 0.04 * sine(432.5, t)
        v = soft_clip(v) * fade_envelope(i, TOTAL_SAMPLES, 3, 3) * 0.7
        samples.append(v)
    return samples


def gen_layer3_celestial():
    """Celestial — 528 Hz + Pythagorean overtones for ethereal feel."""
    samples = []
    for i in range(TOTAL_SAMPLES):
        t = i / SAMPLE_RATE
        # 528 Hz fundamental ("love frequency" per spec)
        v = 0.16 * sine(528.0, t)
        # Pure fifth (792 Hz)
        v += 0.08 * sine(792.0, t)
        # Major third (660 Hz)
        v += 0.06 * sine(660.0, t)
        # Octave above (1056 Hz) for shimmer
        v += 0.05 * sine(1056.0, t)
        # Slow breath (0.05 Hz) modulation
        breath = 0.7 + 0.3 * sine(0.05, t)
        v *= breath
        v = soft_clip(v) * fade_envelope(i, TOTAL_SAMPLES, 3, 3) * 0.6
        samples.append(v)
    return samples


def gen_layer4_combat():
    """Combat — low rumble swell + percussive accents every 4s."""
    samples = []
    for i in range(TOTAL_SAMPLES):
        t = i / SAMPLE_RATE
        # Deep rumble around 80-120 Hz
        v = 0.20 * sine(95.0, t)
        v += 0.15 * sine(48.0, t)
        # Tremolo at 4 Hz (war drum feel)
        trem = 0.6 + 0.4 * sine(4.0, t)
        v *= trem
        # Distorted sub-octave (24 Hz)
        v += 0.10 * math.copysign(1.0, sine(24.0, t))
        # Slow rising tension (1/60 Hz)
        rise = 0.5 + 0.5 * sine(1.0 / 60.0, t)
        v *= rise
        v = soft_clip(v) * fade_envelope(i, TOTAL_SAMPLES, 2, 2) * 0.5
        samples.append(v)
    return samples


def main():
    if not os.path.isdir(OUT_DIR):
        os.makedirs(OUT_DIR, exist_ok=True)
    print(f"[gen_ambient_layers] Output: {OUT_DIR}")
    print(f"[gen_ambient_layers] Duration {DURATION}s @ {SAMPLE_RATE} Hz mono 16-bit PCM")

    for idx, gen in enumerate([gen_layer1_telluric, gen_layer2_harmonic,
                                gen_layer3_celestial, gen_layer4_combat], start=1):
        path = os.path.join(OUT_DIR, f"ambient_layer{idx}.wav")
        print(f"  generating ambient_layer{idx} ...", end="", flush=True)
        samples = gen()
        write_wav(path, samples)
        size = os.path.getsize(path)
        print(f" wrote {size} bytes")
    print("[gen_ambient_layers] Done.")


if __name__ == "__main__":
    main()
