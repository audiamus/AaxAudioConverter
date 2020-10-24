using System.Collections.Generic;
using System.Linq;
using audiamus.aux.ex;

namespace audiamus.aaxconv.lib {
  partial class Book {
    public class ProgressData {

      private readonly static IReadOnlyDictionary<EProgressPhase, int> __phaseWeights = new Dictionary<EProgressPhase, int> {
          { EProgressPhase.none, 0 },
          { EProgressPhase.silence, 3 },
          { EProgressPhase.adjust, 2 },
          { EProgressPhase.transcoding, 2 },
          { EProgressPhase.copying, 1 },
        };

      private readonly static IReadOnlyDictionary<EProgressPhase, int> __phaseWeightsTranscode = new Dictionary<EProgressPhase, int> {
          { EProgressPhase.none, 0 },
          { EProgressPhase.silence, 3 },
          { EProgressPhase.adjust, 2 },
          { EProgressPhase.transcoding, 10 },
          { EProgressPhase.copying, 1 },
        };

      private readonly List<(EProgressPhase phase, int wght)> _weightedPhases = new List<(EProgressPhase, int)> ();
      private int _idx = -1;

      private readonly bool _transcode;
      private readonly int _numParts;

      public EProgressPhase CurrrentPhase => (_idx >= 0 && _weightedPhases.Count > _idx) ? _weightedPhases[_idx].phase : EProgressPhase.none;
      public int CurrentPhaseWeightPart => (_idx >= 0 && _weightedPhases.Count > _idx) ? _weightedPhases[_idx].wght : 0;
      public int CurrentPhaseWeightBook => CurrentPhaseWeightPart * _numParts;

      public ProgressData (int numParts, bool transcode) {
        _numParts = numParts;
        _transcode = transcode;
      }

      public void AddRange (IEnumerable<EProgressPhase> phases) => phases.ForEach (ph => Add (ph));

      public void Add (EProgressPhase phase) {
        var dict = _transcode ? __phaseWeightsTranscode : __phaseWeights;
        int weight = 0;
        dict.TryGetValue (phase, out weight);
        _weightedPhases.Add ((phase, weight));
      }

      public EProgressPhase NextPhase () {
        _idx++;
        return CurrrentPhase;
      }

      public int TotalPhases () => _weightedPhases.Count * _numParts;
      public int TotalPhaseWeights () => _weightedPhases.Select (wp => wp.wght).Sum () * _numParts;
    }
  }
}
