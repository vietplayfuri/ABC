using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABC.Utility
{
    /// <summary>
    /// Fields what not included in real database
    /// </summary>
    public class External : Attribute
    {

    }

    /// <summary>
    /// primary key of table
    /// TODO: support only one primary key in this version
    /// </summary>
    public class PrimaryKey : Attribute
    { }
}
