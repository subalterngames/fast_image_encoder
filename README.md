# Fast Image Encoder

**Blazingly fast C#/Rust zero-allocation thread-safe png/jpg image encoder.**

**Fast Image Encoder is faster than Unity's image encoder by 75% to 98%** (depending on the benchmark used).

This library is meant to speed up projects in which images needed to be encoded to png per-frame as fast as possible, e.g. an ML simulation that either generates image datasets or includes agents that rely on image data.

Unity's own [image encoding methods](https://docs.unity3d.com/ScriptReference/ImageConversion.html) are very slow. They're easy to understand and perfectly adequate for common use-cases such as capturing a screenshot and saving it to disk. For more intensive use-cases, however, the image encoding methods are inefficient. 

Fast Image Encoder speeds up image encoding in two ways:

1. It uses an external Rust library to encode png and jpg files.
2. It encodes to a pre-cached array. Conversely, Unity encodes to a newly-allocated array, thus increasingly GC collect time.

## How to add Fast Image Encoder to your program

1. Compile the Rust crate and copy the library:  `cargo build --release; cp target/release/fast_image_encoder.dll ../../cs/FastImageEncoder/FastImageEncoder`
2. Compile the C# library in `cs/FastImageEncoder/FastImageEncoder` with the Release flag.

1. Add `FastImageEncoder.dll` and `fast_image_encoder.dll` to your project in the same directory. They are both in: `cs/FastImageEncoder/FastImageEncoder/bin/Release`

## Examples and Benchmarks

In the following examples, the raw image data is always 256x256 24-bit RGB.

### 1. Rust-to-Rust: `rs/fast_image_encoder/examples/benchmark`

This program loads raw RGB data in `target/release/examples/images/` and outputs png files in `target/release/examples/images/out`. This is an example of encoding an image within a Rust program. It isn't the intended use-case for this project but it's probably useful for understanding how the Rust code works. It can also be used to benchmark the image encoding without and FFI overhead. 

If you want to encode using images in a pure-Rust application, you shouldn't use this library; just use the `image` crate instead. Fast Image Encoder merely adds a function that allows the `image` crate to interface with an application written in another language.

To compile and run:

1. `cd rs/fast_image_encoder`
2. `cargo build --release --example benchmark; cp -r images/* target/release/examples/images; cargo run --release --example benchmark`

**Average elapsed time per encode:  0.0013 seconds**

### 2. Rust-to-C#: `cs/FastImageEncoder/Benchmark`

This is a minimal example of how to code images in C# using a library built from `rs/fast_image/encoder`. It uses the same raw RGB data as the previous example. It also includes an example of multi-threaded image encoding.

**Average elapsed time per encode: 0.0008 seconds**

I can't explain right now why the C# + Rust code is faster than pure-Rust. It may be a small difference in how each languages' respective stopwatch works. In any case, it's safe to conclude that there is barely any overhead when calling Rust code in C#.

### 3. Rust-to-Unity: `cs/FastImageEncoder/UnityExample`

Before opening the Unity project for the first time, compile the Rust and C# libraries (see above) and copy them into: `cs/FastImageEncoder/UnityExample/Assets/FastImageEncoder/`.

#### 3a. Minimal Example

The MinimalExample scene has a script that generates 10 `ImageEncoders` and encodes each captured image in a separate thread. It's not necessary to use the `ImageEncoder` class; it's mostly minimal boilerplate code. For example, `ImageEncoder` immediately writes the png to disk but you might want to manipulate it before saving it. The `ImageBuffers` class that it references *is* part of Fast Image Encoder and you should always use it.

#### 3b. Benchmark

This Unity scene creates 10 cameras and runs 4 image encoding benchmarks:

1. Sequentially with Unity's image converter
2. In concurrent threads with Unity's image converter
3. Sequentially with Fast Image Encoder
4. In concurrent threads with Fast Image Encoder

There are 10 cameras, so there are 10 renders per frame. 

This benchmark can't be compared directly two previous benchmarks because Unity introduces some overhead and the images are much simpler, which seems to reduce encoding time significantly.

| Threaded | Encoder            | Time elapsed per frame (seconds) | Time elapsed per render (seconds) |
| -------- | ------------------ | -------------------------------- | --------------------------------- |
| No       | Unity              | 0.0420                           | 0.0032                            |
| Yes      | Unity              | 0.0127                           | 0.0053                            |
| No       | Fast Image Encoder | 0.0147                           | 0.0005                            |
| Yes      | Fast Image Encoder | 0.0095                           | 0.0007                            |

**Fast Image Encoder is faster than Unity per-render by approximately 98%.** This ratio will narrow as the complexity of the image increases.

**Using concurrent threads, Fast Image Encoder is faster than Unity per-frame by approximately 77%.**

## Limitations

Fast Image Encoder assumes that the input image data is raw RGB data, as opposed to jpg and png.

## Generate headers

If the Rust code is edited, the C# bindings need to be re-generated:

1. `cd rs/fast_image_encoder`
2. `cargo run --features headers --bin generate-headers`