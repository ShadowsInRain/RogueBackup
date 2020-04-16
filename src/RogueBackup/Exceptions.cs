using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBackup
{
    [Serializable]
    public class BoringException : Exception
    {
        public BoringException() { }
        public BoringException(string message) : base(message) { }
        public BoringException(string message, Exception inner) : base(message, inner) { }
        protected BoringException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    [Serializable]
    public class ImpossibleException : Exception
    {
        public ImpossibleException() { }
        public ImpossibleException(string message) : base(message) { }
        public ImpossibleException(string message, Exception inner) : base(message, inner) { }
        protected ImpossibleException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
