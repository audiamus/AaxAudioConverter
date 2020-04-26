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

  class EnumConverterRM<TEnum> : EnumConverter<TEnum>
     where TEnum : struct, Enum 
  {
    public EnumConverterRM () {
      ResourceManager = this.GetDefaultResourceManager();
    }
  }
}
