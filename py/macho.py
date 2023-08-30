from fat_macho import FatWriter


writer = FatWriter()
with open("../target/aarch64-apple-darwin/release/libfast_image_encoder.dylib", "rb") as f:
    writer.add(f.read())
with open("../target/x86_64-apple-darwin/release/libfast_image_encoder.dylib", "rb") as f:
    writer.add(f.read())
# Get Mach-O fat binary as bytes
fat_bytes = writer.generate()
# Write to file
writer.write_to("../libfast_image_encoder.dylib")
