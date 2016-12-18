using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markdown
{
    public class DocFormatException : ApplicationException
    {
        public DocFormatException(string message) : base(message)
        {
        }
    }
}
