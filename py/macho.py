from fat_macho import FatWriter


writer = FatWriter()
root_dir = "../rs/fast_image_encoder/target/"
with open(f"{root_dir}aarch64-apple-darwin/release/libfast_image_encoder.dylib", "rb") as f:
    writer.add(f.read())
with open(f"{root_dir}x86_64-apple-darwin/release/libfast_image_encoder.dylib", "rb") as f:
    writer.add(f.read())
# Get Mach-O fat binary as bytes
fat_bytes = writer.generate()
# Write to file
writer.write_to("../libfast_image_encoder.dylib")
