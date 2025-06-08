# 📜 Changelog

All notable changes to this project will be documented here.

---

## [v2.0.2] - 2025-06-08

### ⚡ Performance Improvements
- Replaced all uses of `GetPixel` with `LockBits`-based pixel scanning for significantly faster image processing.
- Replaced all calls to `GetColorAtPixel` with `GetColorAtPixelFast` for optimal real-time color detection.
- Optimized pixel search loops to skip redundant checks, greatly improving performance for dense color matching tasks.

### 🔓 Lockpick Hardmode Support
- LockPick module now supports **Hardmode** with precise timing tweaks.
- Introduced fine-tuned `Thread.Sleep` delays between pin detection and click actions (including pre-wait).
- Fully tested and confirmed stable at 80ms delay for all pins (Hardmode safe).

> These changes drastically reduce processing latency and enable perfect timing in fast-paced minigames like **Glass Cutting** and **Lockpicking (Hardmode)**.

---

### 🙏 Special Thanks
**Special thanks to 9 hours of suffering & trial-and-error,** powered by caffeine, frustration, and a burning desire to outsmart virtual locks.

---
