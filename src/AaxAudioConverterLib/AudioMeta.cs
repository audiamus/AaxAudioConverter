namespace audiamus.aaxconv.lib {
  class AudioMeta {
    public readonly TimeInterval Time = new TimeInterval ();
    public uint Channels { get; set; }
    public uint Samplerate { get; set; }
    public uint AvgBitRate { get; set; }
  }

}
