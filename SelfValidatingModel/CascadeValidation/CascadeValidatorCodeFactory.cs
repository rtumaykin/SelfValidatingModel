using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CSharp;

namespace SelfValidatingModel.CascadeValidation
{
    class CascadeValidatorCodeFactory
    {
        const string Lf = "\r\n";
        internal static string GetCode(Type type)
        {
            string _code =
                "using System;" + Lf +
                "using System.Collections;" + Lf +
                "using System.Collections.Generic;" + Lf +
                "using SelfValidatingModel;" + Lf +

                "namespace ns_" + Guid.NewGuid().ToString("N") + Lf +
                "{" + Lf +
                "    [Serializable]" + Lf +
                "    internal class CascadeValidator_" + type.Name + " : SelfValidatingModel.CascadeValidation.ICascadeValidator" + Lf +
                "    {" + Lf +
                "        public IDictionary<string, SelfValidatingModelBase> GetSelfValidatingProperties(SelfValidatingModelBase model)" + Lf +
                "        {" + Lf +
                "            var _trueModel = model as " + type.Namespace + "." + type.Name + ";" + Lf +
                "            if (_trueModel == null)" + Lf +
                "                return null;" + Lf +
                "            return new Dictionary<string, SelfValidatingModelBase>" + Lf +
                "            {" + Lf;
            var _firstEntry = true;

            foreach (var _propertyInfo in type.GetProperties().Where(propertyInfo => propertyInfo.PropertyType.IsSubclassOf(typeof(SelfValidatingModelBase))))
            {
                _code += (_firstEntry ? "" : "," + Lf) + "                    " +
                         "{\"" + _propertyInfo.Name + "\", _trueModel." + _propertyInfo.Name + " }";
                _firstEntry = false;
            }

            _code += Lf + 
                "            };" + Lf +
                "        }" + Lf +
                "    }" + Lf +
                "}";
            return _code;
        }

        internal static string[] GetReferencedAssemblies(Type type)
        {
            return new[]
            {
                type.Assembly.Location, typeof (IDictionary).Assembly.Location,
                typeof (Dictionary<int, int>).Assembly.Location, typeof (DictionaryEntry).Assembly.Location,
                typeof (SelfValidatingModelBase).Assembly.Location
            };
        }

        internal static ICascadeValidator CompileAndGetInstanceOfValidator(Type type)
        {
            var _code = GetCode(type);
            var _referencedAssemblies = GetReferencedAssemblies(type);

            var _compilerParameters = new CompilerParameters
            {
                CompilerOptions = "/t:library",
                GenerateExecutable = false,
                GenerateInMemory = true
            };
            _compilerParameters.ReferencedAssemblies.AddRange(_referencedAssemblies);
//            _compilerParameters.TempFiles.KeepFiles = true;

            var _providerOptions = new Dictionary<string, string> { { "CompilerVersion", "v4.0" } };

            using (var _codeProvider = new CSharpCodeProvider(_providerOptions))
            {
                var _results = _codeProvider.CompileAssemblyFromSource(_compilerParameters, _code);

                if (_results.Errors.Count == 0)
                {
                    var _assembly = _results.CompiledAssembly;
                    Type _validatorType =
                        _assembly.GetTypes().FirstOrDefault(
                            // I don't think there would be any other types in this assembly, but what if compiler decides to add some?
                            info => info.Name.Substring(0, "CascadeValidator_".Length) == "CascadeValidator_");
                    if (_validatorType != null)
                    {
                        var _validatorConstructorInfo = _validatorType.GetConstructor(new Type[] { });
                        
                        if (_validatorConstructorInfo == null)
                            throw new Exception("Failed to find constructor of the generated class");

                        return _validatorConstructorInfo.Invoke(new Object[] { }) as ICascadeValidator;
                    }
                }
                else
                {
                    throw new CascadeValidatorCodeFactoryException(_results.Errors);
                }
            }
            // should not even reach here
            throw new Exception("Unknown error");
        }
    }
}
