# Neon Jackpot

A compact, playable Unity slot machine made with the supplied Slot Machine artwork. It has three independently animated reels, a credit balance, adjustable virtual bets, and a payout for three matching symbols.

## Open and play

1. Install Unity **2022.3 LTS** (the project was set up for 2022.3.62f1).
2. In Unity Hub, choose **Add > Add project from disk** and select this folder.
3. Open `Assets/Scenes/SlotMachine.unity` and press **Play**.
4. Choose a bet with `-` and `+`, then press **SPIN**. `+500` adds demo credits.

All credits are virtual. The game has no purchases, cash value, or network services.

## WebGL build

The Unity editor is required to produce a real WebGL player; no Unity editor is available in the authoring environment, so this repository does **not** contain a compiled WebGL build yet.

To create one in Unity:

1. Install **WebGL Build Support** for Unity 2022.3 LTS in Unity Hub.
2. Open **File > Build Settings**, select **WebGL**, and choose **Switch Platform**.
3. Confirm `Assets/Scenes/SlotMachine.unity` is enabled in **Scenes In Build**.
4. Choose **Build**, select `Build/WebGL` as the output folder, and include the generated files when publishing.
5. Serve the folder over HTTP (for example with `python -m http.server`); opening the generated `index.html` directly from disk is not supported by most browsers.

## Payouts

| Three matching symbols | Payout |
| --- | ---: |
| Cherries | 3x bet |
| Lemons | 2x bet |
| Bells | 5x bet |
| Sevens | 10x bet |

Each reel result is drawn independently from the four-symbol set using `System.Random`. Only three identical symbols win; a 7-7-7 result is the jackpot.

## Approach and project layout

- The supplied artwork is loaded from `Assets/Resources/SlotMachine/` and used for the cabinet, background, and four symbols.
- `SlotMachineGame` owns the UI, credit/bet state, spin flow, win rule, and payout table. `SlotReel` owns the clipped reel presentation, symbol cycling, staggered spin, and eased stop.
- Reel motion uses frame-time-based movement, per-reel start delays, and a short smooth settling animation. No animation package or external runtime dependency is needed.
- The UI is built at runtime in code so the provided art can be positioned consistently on different display sizes.

```text
Assets/
  Animations/       Reel motion is implemented with coroutines.
  Prefabs/          Available for reusable editor-authored prefabs.
  Resources/SlotMachine/  Supplied artwork used at runtime.
  Scenes/           SlotMachine.unity, the playable entry scene.
  Scripts/          Game flow and reel animation.
  Sounds/           Reserved for optional audio assets.
  UI/               Runtime-generated Unity UI.
ProjectSettings/
Packages/
```

## GitHub submission

This folder is ready to publish as a Unity project. Creating a public GitHub repository and pushing it requires an authenticated GitHub account and a repository destination; neither is configured in this environment.
