using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedHebrewAlignmentEngine
{
    public interface ILexiconProvider
    {
        bool IsMatch(string lemma, string target);
    }
}
