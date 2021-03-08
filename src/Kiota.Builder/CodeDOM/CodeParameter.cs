﻿using System;

namespace Kiota.Builder
{
    public enum CodeParameterKind
    {
        Custom,
        QueryParameter,
        Headers,
        ResponseHandler,
        RequestBody
    }

    public class CodeParameter : CodeTerminal, ICloneable
    {
        public CodeParameter(CodeElement parent): base(parent)
        {
            
        }
        public CodeParameterKind ParameterKind = CodeParameterKind.Custom;
        public CodeTypeBase Type;
        public bool Optional = false;

        public object Clone()
        {
            return new CodeParameter(Parent) {
                Optional = Optional,
                ParameterKind = ParameterKind,
                Name = Name.Clone() as string,
                Type = Type.Clone() as CodeTypeBase,
            };
        }
    }
}
