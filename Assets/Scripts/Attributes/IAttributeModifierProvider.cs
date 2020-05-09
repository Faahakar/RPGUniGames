
using System.Collections.Generic;

namespace RPG.Attributes
{
    public interface IAttributeModifierProvider
    {
       IEnumerable<float> GetCurrentAdditiveModifiers(CurrentAttribute attribute);
        IEnumerable<float>  GetCurrentPercentageModifiers(CurrentAttribute attribute);
    }

}