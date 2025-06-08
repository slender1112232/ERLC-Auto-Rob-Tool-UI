# 📜 Changelog

All notable changes to this project will be documented here.

---

## [v2.0.2] - 2025-06-08

### ⚡ Performance
- **Replaced `GetPixel` with `LockBits`** for image processing.
- **Replace all `GetColorAtPixel` with `GetColorAtPixelFast`** for better performance and compatibility with updated Screen.cs
  This significantly boosts performance when handling large pixel areas — reducing processing time from hundreds of milliseconds to just a few.  
  Especially effective for fast-moving minigames like glass cutting.

  
