using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LocalizationLib.Exceptions
{
    [System.Serializable] public class LocalizationSingletonIsNullException : System.Exception
    {
        public LocalizationSingletonIsNullException() : base("Localization singleton is not initialized! Use Localization.InitializeLocalizator() to set singleton a non-null value.") { }
    }
    
    [System.Serializable] public class NodeIsOfUnexpectedTypeException : System.Exception
    {
        public NodeIsOfUnexpectedTypeException(LocalizationNodeType expectedType, LocalizationNodeType unexpectedType) : base($"Localization node was not of expected type. Expected type: {expectedType}. Actual type: {unexpectedType}.") { }
    }

    [System.Serializable] public class PathDoesNotExistException : System.Exception
    {
        public PathDoesNotExistException(string path) : base($"Path \"{path}\" does not exist in this localization.") { }
    }

    [System.Serializable] public class InvalidPathSeparatorException : System.Exception
    {
        public InvalidPathSeparatorException(string separator) : base($"This separator is not valid: {separator}.") { }
    }

    [System.Serializable] public class InvalidPathException : System.Exception
    {
        public InvalidPathException(string path) : base($"This path is not syntactically correct: {path}") { }
    }

    [System.Serializable] public class CannotOperateOnPath : System.Exception
    {
        public CannotOperateOnPath(string path) : base($"Cannot operate on this path: \"{path}\"") { }
    }

    [System.Serializable] public class CannotWriteException : System.Exception
    {
        public CannotWriteException() : base("This localizator cannot perform writing operations.") { }
    }

    [System.Serializable] public class CannotReadException : System.Exception
    {
        public CannotReadException() : base("This localizator cannot perform reading operations.") { }
    }

    [System.Serializable] public class CannotWriteLocalizationException : System.Exception
    {
        public CannotWriteLocalizationException(string localization) : base($"Couldn't perform writing operation on this localization: \"{localization}\".") { }
    }

    [System.Serializable] public class CannotReadLocalizationException : System.Exception
    {
        public CannotReadLocalizationException(string localization) : base($"Couldn't perform reading operation on this localization: \"{localization}\".") { }
    }

    [System.Serializable] public class IncorrectFileExtenstionException : System.Exception
    {
        public IncorrectFileExtenstionException(string extension) : base($"File extension must start with a dot. File extension provided: \"{extension}\"") { }
    }
}