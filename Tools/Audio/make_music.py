"""Gera a música de fundo em Assets/_Project/Audio/Music/ (sintetizada aqui, sem assets de terceiros).

GDD §15.6: música de furtividade, calma e na ponta dos pés. Lá menor, 96 bpm, 16 compassos que emendam sem
corte (as caudas das notas do fim voltam para o começo). WAV mono de 16 bits a 22.050 Hz.
Uso: python Tools/Audio/make_music.py
"""
import wave
from pathlib import Path

import numpy as np

OUT = Path(__file__).resolve().parents[2] / "Assets" / "_Project" / "Audio" / "Music"
RATE = 22050
BPM = 96
BEAT = 60.0 / BPM
BARS = 16
BEATS_PER_BAR = 4
TOTAL = int(RATE * BEAT * BEATS_PER_BAR * BARS)
RNG = np.random.default_rng(11)

# Lá menor harmônica: Am, F, G, E, quatro vezes (a última volta com variação na melodia).
CHORDS = [("A", (57, 60, 64)), ("F", (53, 57, 60)), ("G", (55, 59, 62)), ("E", (52, 56, 59))]
PENTATONIC = (69, 72, 74, 76, 79, 81)  # lá menor pentatônica, na região média


def hz(midi):
    return 440.0 * 2 ** ((midi - 69) / 12)


def add(buffer, start_seconds, signal):
    """Soma o sinal no buffer, dando a volta no fim (loop sem corte)."""
    start = int(start_seconds * RATE)
    index = (np.arange(len(signal)) + start) % TOTAL
    np.add.at(buffer, index, signal)


def pluck(midi, seconds, brightness):
    """Pizzicato: senoide com harmônicos que somem rápido."""
    t = np.arange(int(RATE * seconds)) / RATE
    f = hz(midi)
    tone = np.sin(2 * np.pi * f * t) + brightness * np.sin(2 * np.pi * 2 * f * t) * np.exp(-t / 0.05)
    return tone * np.exp(-t / (seconds * 0.35)) * np.minimum(1.0, t / 0.004)


def bell(midi, seconds):
    t = np.arange(int(RATE * seconds)) / RATE
    f = hz(midi)
    tone = np.sin(2 * np.pi * f * t) + 0.35 * np.sin(2 * np.pi * 3.01 * f * t) * np.exp(-t / 0.15)
    return tone * np.exp(-t / (seconds * 0.4)) * np.minimum(1.0, t / 0.01)


def tick(seconds=0.05):
    t = np.arange(int(RATE * seconds)) / RATE
    noise = RNG.uniform(-1.0, 1.0, len(t))
    noise -= np.convolve(noise, np.ones(4) / 4, mode="same")  # tira o grave: chiado de chimbal
    return noise * np.exp(-t / 0.012)


def build():
    music = np.zeros(TOTAL)
    for bar in range(BARS):
        _, (root, third, fifth) = CHORDS[bar % len(CHORDS)]
        bar_start = bar * BEATS_PER_BAR * BEAT
        # Baixo na ponta dos pés: tônica, quinta no contratempo, oitava, quinta.
        for beat, note in ((0.0, root - 12), (1.5, fifth - 12), (2.0, root), (3.0, fifth - 12)):
            add(music, bar_start + beat * BEAT, 0.55 * pluck(note, BEAT * 0.9, 0.5))
        # Acorde curto e baixinho no tempo 2 e 4.
        for beat in (1.0, 3.0):
            for note in (root, third, fifth):
                add(music, bar_start + beat * BEAT, 0.12 * pluck(note + 12, BEAT * 0.6, 0.2))
        # Chimbal nos contratempos.
        for beat in (0.5, 1.5, 2.5, 3.5):
            add(music, bar_start + beat * BEAT, 0.05 * tick())
        # Melodia esparsa de sino, sorteada na pentatônica (sempre a mesma: semente fixa).
        for beat in (0.5, 2.5, 3.5):
            if RNG.random() < 0.55:
                add(music, bar_start + beat * BEAT, 0.22 * bell(RNG.choice(PENTATONIC), BEAT * 2.5))
    return music


def save(name, signal, gain=0.7):
    data = (signal / np.max(np.abs(signal)) * gain * 32767).astype(np.int16)
    OUT.mkdir(parents=True, exist_ok=True)
    with wave.open(str(OUT / f"{name}.wav"), "wb") as file:
        file.setnchannels(1)
        file.setsampwidth(2)
        file.setframerate(RATE)
        file.writeframes(data.tobytes())


if __name__ == "__main__":
    save("Music_Sneak", build())
    print("ok", TOTAL / RATE, "s")
