# 0.1.4

- Reverted the option to flip raw image data because it's faster to do it on the GPU.
- Updated the Unity example to show how to flip the texture.
- Fixed the Ubuntu dockerfile.

# 0.1.3

- Added an option to flip raw image data before encoding.
- Updated GitHub actions to build from Ubuntu 18 from a Docker container (for better Ubuntu support overall) and to create a MacOS library that can run on Intel as well as Silicon.