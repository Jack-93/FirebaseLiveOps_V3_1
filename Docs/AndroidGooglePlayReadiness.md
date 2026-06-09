# Android / Google Play Readiness

## Current Local State

- Android package: `com.DoOurGame.gameliveops`
- Firebase project: `gameliveops-1449b`
- Firebase Android app id: `1:74044493321:android:89a536deb4f2e72e7672d8`
- Release keystore: not configured yet
- Emulator: `Pixel_8_Play_API_35` was created, but hardware/Hyper-V instability caused a hard reboot when launched. Do not use the emulator again until the virtualization issue is handled.

## Debug SHA For Firebase Test Login

Register these in Firebase Console > Project settings > Android app > SHA certificate fingerprints:

- SHA-1: `ED:97:2C:B1:F2:C5:91:92:E2:AC:5B:BC:1F:CE:D0:F5:4F:27:BC:57`
- SHA-256: `43:B7:6A:03:4F:9A:0A:D8:04:E1:B9:D8:2B:C9:A2:8D:F1:EC:5A:99:76:DD:C5:82:AD:91:84:8D:4C:47:B0:FB`

After adding them, download the refreshed `google-services.json` and replace `Assets/google-services.json`.

## Release / Play Console SHA

For public or internal Play testing, Firebase also needs the Google Play App Signing certificate SHA-1 and SHA-256.

Path in Play Console:

`Play Console > app > Setup > App integrity > App signing key certificate`

Add both SHA values to the same Firebase Android app, then download and replace `google-services.json` again.

## Economy / Prototype Flags

The current prototype intentionally grants large starting resources for fast testing:

- `GameBalanceConfig.PrototypeMinimumGold`
- `GameBalanceConfig.PrototypeMinimumGems`

Before real economy testing, closed testing, or release candidate builds, set both values to `0` and rebalance early progression from `GameBalanceConfig`.

Core economy knobs now live in:

`Assets/Scripts/Core/GameBalanceConfig.cs`

Use this file first when changing:

- hero/enemy growth
- upgrade costs
- equipment stats and upgrade costs
- gacha costs and pity
- quest/event/shop/rewarded-ad rewards
- prototype resource grants

## Unity Menu Checks

Use these after Unity finishes compiling:

- `Tools/Validate Core Data`
- `Tools/Validate Android Build Readiness`
- `Tools/Validate Firebase Security Readiness`
- `Tools/Print Android Debug SHA Fingerprints`
- `Tools/Build Android Debug APK`
- `Tools/Build Android Release AAB`

`Tools/Validate Android Build Readiness` will warn if prototype resource grants are still enabled.

## Save / Account Stability Notes

- Guest and Google login both load through `MainGameBootstrap.InitializeSignedInSessionAsync`.
- Player saves use `FirestoreManager.SavePlayerDataAsync`, which retries failed saves and leaves `HasPendingSave` enabled when a save cannot be completed.
- Inventory, mailbox, daily rewards, quests, shop, ads, and purchases should grant locally first and save afterward.
- If Firestore is unavailable in a local/editor-only flow, reward managers now avoid null reference failures where possible.

## Google Play Blocking Items

These require the owner account in Google Play Console:

- Create or select the Play Console app.
- Complete app access, ads declaration, data safety, content rating, target audience, privacy policy, and store listing.
- Configure Play App Signing.
- Create internal testing track.
- Upload the first `.aab`.
- Add Play App Signing SHA-1/SHA-256 to Firebase.

## Emulator Safety Note

The local emulator launch produced `Kernel-Power 41` with `BugCheckCode = 0` and no minidump, which points to a hard reset rather than a normal app crash. Prefer a real Android device for Google login testing. If emulator testing is unavoidable, try a safer mode later:

`emulator -avd Pixel_8_Play_API_35 -no-accel -no-snapshot -gpu off`

This will be slow, but it avoids hardware acceleration.

## Local Batch Build Note

Core validation and Android/Firebase readiness validation pass in Unity batch mode.
The latest debug APK batch build reached `Incremental Player Build` and then stopped producing logs, matching the earlier release AAB stall. The stuck Unity batch process was terminated without deleting source files or build caches.

Before the next APK/AAB attempt:

- Open Unity once and allow all imports to finish.
- Run `Tools/Validate Core Data`.
- Run `Tools/Validate Android Build Readiness`.
- Try the build from the Unity Editor menu.
- If `Incremental Player Build` stalls again, clear only generated build caches or use a clean build on the next development session.

Do not launch the configured Android emulator automatically. Use a real Android device when available.
