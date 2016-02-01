using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Models
{
    public enum TokenGroup
    {
        Unknown,
        ReservedWord,
        Identifier,
        Number,
        Literal,
        Relational,
        Operator,
        SpecialCharacter
    }
}
