// export interface ISearchDocument {
//     id: string,
//     documentNummer:string, 
//     titel: string,
//     onderwerp: string,
//     soort: string,
//     organisatie: string,
//     highlights:string[],
//     datum:Date
// }
export interface ISearchDocument {
    id: string,
    documentId:string,
    fileName:string,
    documentNummer:string, 
    titel: string,
    onderwerp: string,
    soort: string,
    organisatie: string,
    highlights:string[],
    datum:Date,
    content:string,
    chunkId:string
}