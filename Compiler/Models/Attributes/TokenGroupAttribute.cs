using System;

namespace Compiler.Models.Attributes
{
    [System.AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public sealed class TokenGroupAttribute : Attribute
    {
        // See the attribute guidelines at
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        private readonly TokenGroup tokenGroup;

        // This is a positional argument
        public TokenGroupAttribute(TokenGroup tokenGroup)
        {
            this.tokenGroup = tokenGroup;

            // TODO: Implement code here

            throw new NotImplementedException();
        }

        public TokenGroup TokenGroup
        {
            get { return tokenGroup; }
        }
    }
}