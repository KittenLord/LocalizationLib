using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocalizationLib.Exceptions;

namespace LocalizationLib
{
    public class LocalizatorSettings
    {
        public bool CanRead => Reader is not null;
        public ILocalizatorReader Reader { get { if(_reader != null) return _reader; throw new CannotReadException(); } set { _reader = value; } }
        private ILocalizatorReader? _reader;


        public bool CanWrite => Writer is not null;
        public ILocalizatorWriter Writer { get { if(_writer != null) return _writer; throw new CannotWriteException(); } set { _writer = value; } }
        private ILocalizatorWriter? _writer;





        public bool UseSingleFile { get; set; } = false;
        public bool EnableCaching { get; set; } = true;
        public string PathPrefix { get; set; } = "";




        public LocalizatorSettings(ILocalizatorReader reader, ILocalizatorWriter writer)
        {
            Reader = reader;
            Writer = writer;
        }

        public LocalizatorSettings(ILocalizatorReader reader)
        {
            Reader = reader;
        }
        
        public LocalizatorSettings(ILocalizatorWriter writer)
        {
            Writer = writer;
        }

        public LocalizatorSettings() { }
    }
}