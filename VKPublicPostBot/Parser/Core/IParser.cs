using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VKPublicPostBot.Parser.Core
{
    internal interface IParser<T>
    {
         Task<T> Parse(IElement element);
    }
}
