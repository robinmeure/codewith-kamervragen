// this class is used to download data and strongly type it into a list of TweedeKamerDocument objects
// see for the source of the documents and their properties at https://gegevensmagazijn.tweedekamer.nl/OData/v4/2.0/Document

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Kamervragen.Domain.Blob
{
    public class ODataResponse
    {
        public List<TweedeKamerDocument> Value { get; set; }
        [JsonPropertyName("@odata.nextLink")]
        public string? NextLink { get; set; }
    }
    public class TweedeKamerDocument
    {
        public required string Id { get; init; }
        public required string Soort { get; init; }
        public required string DocumentNummer { get; init; }
        public string? Titel { get; init; }
        public required string Onderwerp { get; init; }
        public required string Datum { get; init; }
        public required string Vergaderjaar { get; init; }
        public int Kamer { get; init; }
        public int Volgnummer { get; init; }
        public string? Citeertitel { get; init; }
        public string? Alias { get; init; }
        public DateTime DatumRegistratie { get; init; }
        public DateTime? DatumOntvangst { get; init; }
        public string? Aanhangselnummer { get; init; }
        public string? KenmerkAfzender { get; init; }
        public string? Organisatie { get; init; }
        public string? ContentType { get; init; }
        public int ContentLength { get; init; }
        public DateTime GewijzigdOp { get; init; }
        public DateTime ApiGewijzigdOp { get; init; }
        public bool Verwijderd { get; init; }
        public string? HuidigeDocumentVersie_Id { get; init; }
    }
}


