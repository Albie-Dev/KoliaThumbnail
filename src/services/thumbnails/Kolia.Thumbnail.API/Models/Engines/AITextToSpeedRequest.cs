namespace Kolia.Thumbnail.API.Engines
{
    // ===================== Text to Speech =====================

    /// <summary>
    /// Request chuyển văn bản thành giọng nói.
    /// </summary>
    public class TextToSpeechRequest
    {
        public string Model { get; set; } = default!;
        public string ApiKey { get; set; } = default!;
        public string Text { get; set; } = default!;
        public string VoiceId { get; set; } = default!;

        /// <summary>Tốc độ đọc (1.0 = bình thường).</summary>
        public double Speed { get; set; } = 1.0;

        /// <summary>Cao độ giọng (nếu provider hỗ trợ, 1.0 = mặc định).</summary>
        public double? Pitch { get; set; }

        /// <summary>Mã ngôn ngữ, vd "vi-VN", "en-US" (một số provider yêu cầu bắt buộc).</summary>
        public string? LanguageCode { get; set; }

        public AudioOutputFormat OutputFormat { get; set; } = AudioOutputFormat.Mp3;

        /// <summary>Sample rate mong muốn (Hz), null = mặc định của provider.</summary>
        public int? SampleRate { get; set; }

        /// <summary>
        /// Hướng dẫn phong cách giọng đọc bằng văn bản tự nhiên (vd: "đọc vui vẻ, năng lượng cao")
        /// - chỉ 1 số provider mới (OpenAI TTS, ElevenLabs v3) hỗ trợ.
        /// </summary>
        public string? StyleInstruction { get; set; }
    }

    public class TextToSpeechResult
    {
        public byte[] AudioBytes { get; set; } = default!;
        public AudioOutputFormat Format { get; set; }
        public double DurationSeconds { get; set; }

        /// <summary>
        /// Timestamp theo từng từ (word-level) - hữu ích để đồng bộ phụ đề với audio đã sinh.
        /// </summary>
        public List<WordTimestamp>? WordTimestamps { get; set; }
    }

    public class WordTimestamp
    {
        public string Word { get; set; } = default!;
        public double StartSeconds { get; set; }
        public double EndSeconds { get; set; }
    }

    /// <summary>
    /// Thông tin 1 giọng đọc khả dụng của provider.
    /// </summary>
    public class AIVoiceInfo
    {
        public string VoiceId { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Language { get; set; }
        public string? Gender { get; set; }
        public string? Accent { get; set; }
        public string? PreviewUrl { get; set; }

        /// <summary>Giọng do người dùng tự clone (nếu provider hỗ trợ voice cloning).</summary>
        public bool IsCustomCloned { get; set; }
    }

    public enum AudioOutputFormat
    {
        Mp3 = 0,
        Wav = 1,
        Ogg = 2,
        Flac = 3,
        Aac = 4
    }

    // ===================== Speech to Text =====================

    /// <summary>
    /// Request nhận diện giọng nói thành văn bản.
    /// </summary>
    public class SpeechToTextRequest
    {
        public string Model { get; set; } = default!;
        public string ApiKey { get; set; } = default!;

        public byte[] AudioBytes { get; set; } = default!;

        /// <summary>Định dạng file audio đầu vào (mp3, wav, m4a...).</summary>
        public string InputFormat { get; set; } = "mp3";

        /// <summary>Mã ngôn ngữ audio, để null để provider tự phát hiện.</summary>
        public string? Language { get; set; }

        /// <summary>Gợi ý ngữ cảnh/từ vựng chuyên ngành giúp tăng độ chính xác.</summary>
        public string? Prompt { get; set; }

        /// <summary>Trả về timestamp theo từng câu/từ - dùng để tạo phụ đề (srt/vtt).</summary>
        public bool IncludeTimestamps { get; set; }

        /// <summary>Tự động phân biệt người nói (diarization) - nếu provider hỗ trợ.</summary>
        public bool EnableSpeakerDiarization { get; set; }
    }

    public class SpeechToTextResult
    {
        public string Text { get; set; } = default!;
        public string? DetectedLanguage { get; set; }

        public List<TranscriptSegment>? Segments { get; set; }

        /// <summary>Độ tin cậy trung bình của kết quả (0.0 - 1.0), nếu provider trả về.</summary>
        public double? Confidence { get; set; }
    }

    /// <summary>
    /// 1 đoạn transcript có timestamp - dùng trực tiếp để build file phụ đề .srt/.vtt.
    /// </summary>
    public class TranscriptSegment
    {
        public string Text { get; set; } = default!;
        public double StartSeconds { get; set; }
        public double EndSeconds { get; set; }

        /// <summary>Nhãn người nói (vd: "Speaker 1") - nếu bật EnableSpeakerDiarization.</summary>
        public string? SpeakerLabel { get; set; }
    }
}