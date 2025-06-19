// Exports.cs
// NativeAOT entry-points for Dart FFI.
// Build with: <PublishAot>true</PublishAot>  +  <NativeLib>Shared</NativeLib>

using System;
using System.Runtime.InteropServices;
using NeMoOnnxSharp.Models;      // EncDecCTCModel, EncDecCTCConfig
using NeMoOnnxSharp;             // WaveFile utility

public static unsafe class Exports
{
    /* ────────────────────────── 1. InitSession ──────────────────────────
       nuint InitSession(const char* onnxPathUtf8, int vocabId)
       - onnxPathUtf8 : UTF-8 filepath on device
       - vocabId      : 0 = EnglishVocabulary, 1 = BambaraVocabulary
       ← opaque handle sent back to Dart (store in a nuint / int).          */
    [UnmanagedCallersOnly(EntryPoint = "InitSession")]
    public static nuint InitSession(byte* onnxUtf8) // Remove vocabId later
    {
        var modelPath = Marshal.PtrToStringUTF8((nint)onnxUtf8)!;

        var cfg = new EncDecCTCConfig
        {
            modelPath  = modelPath,
            vocabulary = EncDecCTCConfig.BambaraVocabulary
        };

        var model   = new EncDecCTCModel(cfg);
        var gch     = GCHandle.Alloc(model);          // pin the object
        return (nuint)GCHandle.ToIntPtr(gch);         // opaque handle → Dart
    }

    /* ──────────────────────── 2. TranscribePath ────────────────────────
       char* Transcribe(nuint handle, const char* wavPathUtf8)
       - handle       : value from InitSession
       - wavPathUtf8  : UTF-8 path to a PCM WAV file (16-bit, any SR/ch)
       ← malloc-ed UTF-8 string; free with FreeCString                   */
    [UnmanagedCallersOnly(EntryPoint = "Transcribe")]
    public static byte* Transcribe(nuint handle, byte* wavUtf8)
    {
        var model = (EncDecCTCModel)GCHandle.FromIntPtr((nint)handle).Target!;
        var wavPath = Marshal.PtrToStringUTF8((nint)wavUtf8)!;

        // Load → resample → mono
        short[] pcm = WaveFile.ReadWAV(wavPath, model.SampleRate);

        // Full pipeline (preprocess + ONNX + greedy CTC)
        string text = model.Transcribe(pcm);

        // Return as unmanaged UTF-8
        return (byte*)Marshal.StringToCoTaskMemUTF8(text);
    }

    /* ───────────────────────── 3. FreeCString ─────────────────────────── */
    [UnmanagedCallersOnly(EntryPoint = "FreeCString")]
    public static void FreeCString(byte* ptr) => Marshal.FreeCoTaskMem((nint)ptr);

    /* ────────────────────────── 4. Dispose ───────────────────────────── */
    [UnmanagedCallersOnly(EntryPoint = "Dispose")]
    public static void Dispose(nuint handle)
        => GCHandle.FromIntPtr((nint)handle).Free();
}
