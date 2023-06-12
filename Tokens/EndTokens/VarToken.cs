using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyBasic.Tokens.BaseTokens;
using TinyBasic.Tokens.CharTokens;

namespace TinyBasic.Tokens.EndTokens
{
    internal class VarToken : IToken, IFactorArg
	{
        public VariableName Name;
        public enum VariableName
        {
            A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z
        }
    }

    internal class VarListToken : IToken
    {
        public VarToken FirstVar { get; set; }
        public List<(CommaToken, VarToken)> Values { get; set; } = new();
    }
}
