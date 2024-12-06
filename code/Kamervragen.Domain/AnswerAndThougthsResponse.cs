using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamervragen.Domain
{
    public class AnswerAndThougthsResponse
    {
        public required string Answer { get; set; }
        public required string Thoughts { get; set; }
    }
}
