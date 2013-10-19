using System;
using System.CodeDom.Compiler;

namespace SelfValidatingModel.CascadeValidation
{
    public sealed class CascadeValidatorCodeFactoryException : Exception
    {
        internal CascadeValidatorCodeFactoryException(CompilerErrorCollection errorCollection)
        {
             Data.Add("Compiler Errors", errorCollection);
        }
    }
}
