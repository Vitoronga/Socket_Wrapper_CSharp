using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketWrapperLibrary
{
    public interface ISocketMessage
    {
        public byte ClassId { get; }

        public byte[] FormatDataAsByteArray();
    }
}
