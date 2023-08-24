# Fast Image Encoder

**Blazingly fast C# zero-allocation thread-safe png/jpg image encoding using an external dll written in Rust. **

Fast Image Encoder is a C#/Rust image encoding library. It is meant as a significantly faster alternative to Unity's image encoding methods.

## How to add this to your program

1. Compile the Rust crate and copy the dll:  `cargo build --release; cp target/release/fast_image_encoder.dll cs/FastImageEncoder/FastImageEncoder`
2. Compile the C# dll in `cs/FastImageEncoder/FastImageEncoder` with the Release flag.

1. Add `FastImageEncoder.dll` and `fast_image_encoder.dll` to your project in the same directory. They are both in: `cs/FastImageEncoder/FastImageEncoder/bin/Release`

## Examples

### 1. Rust-to-Rust

This is an example of encoding an image within a Rust program. It isn't the intended use-case for this project but it's probably useful for understanding how the Rust code works. It can also be used to benchmark the image encoding without and FFI overhead.

To compile and run:

1. `cd rs/fast_image_encoder`
2. `cargo build --release --example benchmark; cp -r images/* target/release/examples/images; cargo run --release --example benchmark`

## The problem that this solves

There are two problems:

1. Unity's own [image encoding methods](https://docs.unity3d.com/ScriptReference/ImageConversion.html) are very slow. There's not much more to be said because it's black-box code.
2. By returning byte arrays, these methods also do an extra memory allocation operation that could technically be skipped--there's no algorithmic reason why we can't just encode in-place to a pre-cached array. As is, every encoded image is a separate array that can end up in the garbage collector. This can add up quickly, especially if you're encoding multiple images per frame.

## How this solves the problem

### 1. Rust

Instead of using Unity's image encoding, we use a dll written in Rust that encodes images using the [`image`](https://docs.rs/image/latest/image/) crate. If called from a Rust binary, this is around four times faster than the Unity C# image encoding (though there will be some overhead later).

### 2. Zero-allocation

ImageEncoder allocates two arrays for image encoding: a raw images array and an encoded image array. They are the same size. Because encoded images are always smaller than raw images, we know we can always write an encoded image into the encoded image array.

### 3. Relative safety

FFI programs are inherently unsafe, but some are safer than others. In this case, we never transfer ownership of anything to Rust: the Rust library merely accepts references to the pre-allocated arrays, which are managed in the C# code.

### 4. Thread safety

Because the code is safe and because the code never touches the Unity thread, we can safely encode multiple images concurrently within C#. 

## Limitations

Fast Image Encoder assumes that the input image data is raw RGB data, as opposed to jpg and png.

# Benchmark

### 1. TDW (threading enabled)

```python
from math import sin, cos, pi
from tdw.controller import Controller
from tdw.add_ons.third_person_camera import ThirdPersonCamera
from tdw.add_ons.benchmark import Benchmark
from tdw.output_data import OutputData, Images
from tdw.tdw_utils import TDWUtils

c = Controller()
benchmark = Benchmark()
camera = ThirdPersonCamera(avatar_id="a",
                           position={"x": 0, "y": 1, "z": 0})
c.add_ons.extend([camera, benchmark])
c.communicate(Controller.get_add_scene(scene_name="tdw_room"))
commands = [{"$type": "set_pass_masks",
             "pass_masks": ["_img", "_id", "_category", "_mask", "_normals", "_albedo"]},
            {"$type": "send_images",
             "frequency": "always"}]
n = 15
da = (2 * pi) / n
angle = 0
r = 1.75
for i in range(n):
    x = cos(angle) * r
    z = sin(angle) * r
    commands.append(Controller.get_add_object(model_name="chair_billiani_doll",
                                              object_id=Controller.get_unique_id(),
                                              position={"x": x, "y": 0, "z": z}))
    angle += da
c.communicate(commands)
benchmark.start()
for i in range(360):
    camera.rotate({"x": 0, "y": 1, "z": 0})
    resp = c.communicate([])
benchmark.stop()
c.communicate({"$type": "terminate"})
print(benchmark.fps, sum(benchmark.times))
```

And:

```python
import re
from tdw.backend.paths import PLAYER_LOG_PATH


text = PLAYER_LOG_PATH.read_text()
per_pass = re.findall("Per-pass:\n(.*)\n(.*)\n(.*)\n(.*)\n(.*)\n(.*)", text, flags=re.MULTILINE)
q = list()
for pp in per_pass:
    q.extend([float(x) for x in pp])
print("Per pass (average):", sum(q) / len(q))
totals = re.findall("total: (.*)", text, flags=re.MULTILINE)
totals = [float(t) for t in totals]
print("Totals (average):", sum(totals) / len(totals))
```

Output:

```
Per pass (average): 0.004009581445672194
Totals (average): 0.006808582044198897
```

## 2. EncoderTest

```
Sequential encoding:
0.002483
0.001102
0.0012888
0.0012619
0.0011319
0.001191
Threaded encoding:
0.0047625
```

## 3. Rust

```bash
cargo test --release -- --nocapture
```

Output:

```
0.00135
0.001139
0.001697
0.001621
0.001162
0.001252
```

This tells us that the C# overhead in EncoderTest is nearly zero.

## Generate headers

If the Rust code is edited, the C# bindings need to be re-generated:

1. `cd rs/fast_image_encoder`
2. `cargo run --features headers --bin generate-headers`