using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreamReader.Container
{
    public interface IContainer
    {
        FST IsContainer(StreamReader.IReader fileReader, FST.cFile file, int index);
    }
}
