// export interface ISearchDocument {
//     id: string,
//     documentNummer:string, 
//     titel: string,
//     onderwerp: string,
//     soort: string,
//     organisatie: string,
//     highlights:string[],
//     datum:Date

import { IQuestions_And_Answers } from "./DocumentResult";

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
    chunkId:string,
    summary:string,
    questionandanswers:IQuestions_And_Answers[],
    intent:string,
    members:string
}