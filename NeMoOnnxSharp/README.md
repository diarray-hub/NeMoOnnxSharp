# Android NativeAOT Build & Export Guide – NeMoOnnxSharp

## NativeAOT Build Instructions

### Build for Linux (x64)

Quite Straightforward for most Linux desktop:

```bash
dotnet publish -c Release -r linux-x64
```

---

### Build for Android Native (arm64, arm, x64)

.NET 8 does **not** natively support AOT for android-arm64 targets. Here’s the workaround, based on [HelloJniLib](https://github.com/josephmoresena/NativeAOT-AndroidHelloJniLib/):

1. **Install Android NDK** (r26b or r27c).

   ```bash
   export ANDROID_NDK_ROOT=/path/to/android-ndk-r27c
   ```
2. Copy [`BionicNativeAot.targets`](https://github.com/josephmoresena/NativeAOT-AndroidHelloJniLib/blob/master/BionicNativeAot.targets) into your repo.
3. Import this file in your `.csproj`.
4. Build for Android-compatible target:

   ```bash
   dotnet publish -c Release -r linux-bionic-arm64 \
     -p:DisableUnsupportedError=true \
     -p:PublishAotUsingRuntimePack=true \
     -p:AssemblyName=libNeMoOnnxSharp
   ```

Output: `libNeMoOnnxSharp.so` in your publish folder. Place this in your Android app’s `jniLibs/arm64-v8a/` folder.

**NOTE:** This will not run on iOS. Cross-compile for Android only.

---

## Exported C FFI Functions (Exports.cs)

To make the native ASR model accessible from Dart/Flutter (or any C FFI), we exported several functions from `Exports.cs`:

### Main exports (all \[UnmanagedCallersOnly])

* **`nuint initSession(const char* modelPath)`**

  * Loads the ONNX model and returns a session handle.
  * Use this handle for all future transcription calls.

* **`IntPtr transcribe(nuint sessionHandle, const char* wavPath)`**

  * Runs the ASR model on the provided .wav audio file path.
  * Returns a C string (pointer) with the transcription result.

* **`void freeCString(IntPtr ptr)`**

  * Frees the memory of the C string returned by `transcribe`.
  * Must be called from Dart/FFI to avoid memory leaks.

* **`void disposeSession(nuint sessionHandle)`**

  * Disposes and releases resources for the ASR session.

### Why this API?

* All functions are C-compatible and can be called from Flutter/Dart FFI or any native code.
* Designed to keep a native session alive for fast multiple calls.
* String pointers require explicit free (see `freeCString`).

### Usage Example (in Dart)

```dart
final session = initSession(modelPath.toNativeUtf8());
final resultPtr = transcribe(session, wavPath.toNativeUtf8());
final result = resultPtr.toDartString();
freeCString(resultPtr);
disposeSession(session);
```

---

## Credits

* NativeAOT Android hack: [HelloJniLib](https://github.com/josephmoresena/NativeAOT-AndroidHelloJniLib/)
* ONNX ASR C# implementation: [kaiidams/NeMoOnnxSharp](https://github.com/kaiidams/NeMoOnnxSharp)

---
