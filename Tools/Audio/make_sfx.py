"""Gera os efeitos sonoros do jogo em Assets/_Project/Audio/SFX/ (sintetizados aqui, sem assets de terceiros).

GDD §15.6. WAV mono de 16 bits a 22.050 Hz, curtos e leves para o WebGL.
Uso: python Tools/Audio/make_sfx.py
"""
import wave
from pathlib import Path

import numpy as np

OUT = Path(__file__).resolve().parents[2] / "Assets" / "_Project" / "Audio" / "SFX"
RATE = 22050
RNG = np.random.default_rng(7)


def timeline(seconds):
    return np.arange(int(RATE * seconds)) / RATE


def envelope(t, attack, decay):
    """Sobe em attack segundos e cai exponencialmente com a constante decay."""
    rise = np.clip(t / max(attack, 1e-4), 0.0, 1.0)
    return rise * np.exp(-np.maximum(t - attack, 0.0) / decay)


def sweep(t, start_hz, end_hz):
    """Senoide com a frequência deslizando de start_hz a end_hz."""
    freq = np.linspace(start_hz, end_hz, len(t))
    return np.sin(2.0 * np.pi * np.cumsum(freq) / RATE)


def lowpass(signal, strength):
    """Filtro passa-baixa de um polo (strength de 0 a 1: maior = mais abafado)."""
    out = np.empty_like(signal)
    acc = 0.0
    for i, x in enumerate(signal):
        acc = acc * strength + x * (1.0 - strength)
        out[i] = acc
    return out


def noise(n):
    return RNG.uniform(-1.0, 1.0, n)


def save(name, signal, gain=0.8):
    peak = np.max(np.abs(signal)) or 1.0
    fade = np.minimum(1.0, np.linspace(1.0, 0.0, len(signal)) * 40.0)  # sem estalo no fim
    data = (signal / peak * gain * fade * 32767).astype(np.int16)
    OUT.mkdir(parents=True, exist_ok=True)
    with wave.open(str(OUT / f"{name}.wav"), "wb") as file:
        file.setnchannels(1)
        file.setsampwidth(2)
        file.setframerate(RATE)
        file.writeframes(data.tobytes())


def run_step():
    t = timeline(0.12)
    thump = np.sin(2 * np.pi * 110 * t) * envelope(t, 0.003, 0.03)
    scuff = lowpass(noise(len(t)), 0.85) * envelope(t, 0.002, 0.02)
    save("Sfx_RunStep", thump + 1.5 * scuff, 0.55)


def whistle():
    t = timeline(0.5)
    first = t < 0.2
    freq_a = sweep(t, 1500, 2100)
    freq_b = sweep(t, 2100, 1700)
    tone = np.where(first, freq_a, freq_b) * (1 + 0.03 * np.sin(2 * np.pi * 9 * t))
    amp = np.where(first, envelope(t, 0.02, 0.6), envelope(t - 0.22, 0.02, 0.15) * (t > 0.22))
    save("Sfx_Whistle", tone * amp + 0.05 * lowpass(noise(len(t)), 0.5) * amp, 0.6)


def pickup():
    t = timeline(0.16)
    save("Sfx_Pickup", sweep(t, 520, 1150) * envelope(t, 0.005, 0.06), 0.6)


def drop():
    t = timeline(0.22)
    thud = np.sin(2 * np.pi * 85 * t) * envelope(t, 0.002, 0.06)
    knock = lowpass(noise(len(t)), 0.7) * envelope(t, 0.001, 0.03)
    save("Sfx_Drop", thud + knock, 0.7)


def glass_break():
    t = timeline(0.7)
    crash = (noise(len(t)) - lowpass(noise(len(t)), 0.6)) * envelope(t, 0.001, 0.08)
    shards = np.zeros_like(t)
    for _ in range(9):
        start = RNG.uniform(0.0, 0.25)
        freq = RNG.uniform(2500, 5200)
        local = np.maximum(t - start, 0.0)
        shards += np.sin(2 * np.pi * freq * local) * envelope(local, 0.001, RNG.uniform(0.04, 0.12)) * (t >= start)
    save("Sfx_GlassBreak", crash + 0.5 * shards, 0.7)


def deliver():
    t = timeline(0.6)
    note_a = np.sin(2 * np.pi * 988 * t) + 0.3 * np.sin(2 * np.pi * 1976 * t)
    note_b = np.sin(2 * np.pi * 1319 * t) + 0.3 * np.sin(2 * np.pi * 2638 * t)
    second = t >= 0.09
    signal = note_a * envelope(t, 0.003, 0.08) * ~second + note_b * envelope(t - 0.09, 0.003, 0.2) * second
    save("Sfx_Deliver", signal, 0.6)


def spotted():
    t = timeline(0.32)
    square = lambda f: np.sign(np.sin(2 * np.pi * f * t))  # noqa: E731
    second = t >= 0.11
    signal = square(880) * envelope(t, 0.002, 0.06) * ~second + square(1175) * envelope(t - 0.11, 0.002, 0.1) * second
    save("Sfx_Spotted", lowpass(signal, 0.55), 0.5)


def caught():
    t = timeline(0.9)
    saw = 2 * ((np.cumsum(np.linspace(440, 180, len(t))) / RATE) % 1.0) - 1
    wobble = 1 + 0.35 * np.sin(2 * np.pi * 6 * t)
    save("Sfx_Caught", lowpass(saw * wobble, 0.75) * envelope(t, 0.01, 0.5), 0.6)


def win():
    t = timeline(1.1)
    signal = np.zeros_like(t)
    for i, freq in enumerate((523, 659, 784, 1047)):
        start = i * 0.11
        local = np.maximum(t - start, 0.0)
        tone = np.sin(2 * np.pi * freq * local) + 0.25 * np.sin(2 * np.pi * 2 * freq * local)
        signal += tone * envelope(local, 0.004, 0.18 if i < 3 else 0.5) * (t >= start)
    save("Sfx_Win", signal, 0.65)


def bin_clank():
    t = timeline(0.4)
    signal = sum(np.sin(2 * np.pi * f * t) * envelope(t, 0.001, d) for f, d in ((310, 0.15), (780, 0.08), (1240, 0.05)))
    signal += 0.6 * lowpass(noise(len(t)), 0.4) * envelope(t, 0.001, 0.02)
    save("Sfx_BinClank", signal, 0.6)


def ui_select():
    t = timeline(0.05)
    save("Sfx_UISelect", np.sin(2 * np.pi * 1700 * t) * envelope(t, 0.001, 0.012), 0.4)


if __name__ == "__main__":
    for make in (run_step, whistle, pickup, drop, glass_break, deliver, spotted, caught, win, bin_clank, ui_select):
        make()
    print("ok", sorted(p.name for p in OUT.glob("*.wav")))
