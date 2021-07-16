﻿namespace BrightChain.Engine.Exceptions
{
    /// <summary>
    /// Base class for all BrightChain exceptions
    /// </summary>
    public class BrightChainValidationException : BrightChainException
    {
        public string Element { get; protected set; }

        public BrightChainValidationException(string element, string message) : base(message)
        {
            Element = element;
        }

        public BrightChainValidationException(string element, object _) : base("BOGUS!")
        {
            Element = element;
        }
    }
}