using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kamervragen.Domain.Chat
{
    public record SelectedQAPair
    {
        public required string DocumentId { get; set; }
        public required string Question { get; set; }
        public required string Answer { get; set; }
    }
}
