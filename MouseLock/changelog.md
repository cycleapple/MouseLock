# API 13 port
- Ported the plugin lifecycle from API 15's asynchronous interface to API 13.
- Added API 13-compatible camera, UI input, mouse drag, and cursor recenter bindings.
- Added the standalone manifest required by the API 13 packager.

# 1.0.0.3
- Added a true toggle option for tap release, so the cursor can stay free until you tap the modifier again.
- The existing “release until world click or next tap” behavior is still available and remains the default.
- Made tap release safer around Alt+Tab and other modifier combinations.
- Clarified several settings descriptions, especially tap release, resume behavior, and LMB + RMB movement.

# 1.0.0.2
- Implement native LMB+RMB movement (this disables other LMB/RMB bindings).
- Fix a bunch of misc bugs.

# 1.0.0.1
- Made some performance optimizations

# 1.0.0.0
- Initial Release
