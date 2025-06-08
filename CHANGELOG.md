# 📜 Changelog

All notable changes to this project will be documented here.

---

## \[v2.1.0] - 2025-06-09

### 🎯 Functional Updates

* Added **LockPick hardmode** support with precise timing for fast pin detection.
* Added cooldown tracking for **LockPick**, consistent with other modules (ATM, Glass Cutting, etc.).
* Each module now uses **independent device context (DC)** to prevent shared resource conflicts.
* Delayed DC initialization to occur **after game focus + delay**, improving stability at first run.
  *(If you experienced wild mouse clicking or failures during the first LockPick attempt — yes, this was the cause.)*

### 🛠 Bug Fixes

* Fixed issue where first LockPick attempt would fail due to DC being initialized too early.
* Fixed mouse glitch after finishing Glass Cutting.
* Removed legacy `SpamClickToFail()` logic from LockPick (no longer needed or used).
* Actions can now be triggered during cooldown; triggering again resets the cooldown.

---

### ⚙ Internal Code Maintenance

* Reworked `Screen.cs` to support controlled `Init()` / `ReleaseDC()` calls.
* Refactored LockPicking logic for cleaner per-pin handling and Y-sweep detection logic.
* Improved log traceability during pixel scan and click phases.

---

### 🙏 Special Thanks

**Shout-out to late-night testing sessions, random game behavior, and the first-run curse.**
Stability earned the hard way. 💻☕🔓


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
