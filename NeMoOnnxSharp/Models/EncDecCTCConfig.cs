// Copyright (c) Katsuya Iida.  All Rights Reserved.
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace NeMoOnnxSharp.Models
{
    public class EncDecCTCConfig : ModelConfig
    {
        public const string EnglishVocabulary = " abcdefghijklmnopqrstuvwxyz'_";
        public const string GermanVocabulary = " abcdefghijklmnopqrstuvwxyzäöüß_";
        // Add the QuartzNet vocabulary after Bambara Training
        // Reminder: This might need update after further training
        // "_" represents the blank token in CTC
        public const string BambaraVocabulary = "0123456789abcdefghijklmnopqrstuvwxyz '-ŋɔɛɲɓɾ_";

        public string? vocabulary;
    }
}
