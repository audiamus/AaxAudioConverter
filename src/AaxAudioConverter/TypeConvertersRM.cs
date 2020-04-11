using System;
using audiamus.aux;
using audiamus.aux.ex;

namespace audiamus.aaxconv {
  class BooleanYesNoConverterRM : BooleanYesNoConverter {
    public BooleanYesNoConverterRM () {
      ResourceManager = this.GetDefaultResourceManager();
    }
  }

  class EnumChainConverterRM<TEnum, TPunct> : EnumChainConverter<TEnum, TPunct>
    where TEnum : struct, Enum
    where TPunct : class, IChainPunctuation, new() 
  {
    public EnumChainConverterRM () {
      ResourceManager = this.GetDefaultResourceManager();
    }
  }

  class LongTitleConverterRM : LongTitleConverter {
    public LongTitleConverterRM () {
      ResourceManager = this.GetDefaultResourceManager();
    }
  }
}
