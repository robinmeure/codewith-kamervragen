import { IQuestions_And_Answers } from "./DocumentResult";

export interface IChatMessage {
    id?: string,
    role: string,
    content: string,
    timestamp?: string,
    context?:IChatContext    
}

export interface IChatContext
{
    followup_questions?: string[],
    citations?: Citation[],
    dataPointsContent?: DataPointsContent[],
    thoughts?: Thoughts[]
}

export type Thoughts = {
    title: string;
    description: any; // It can be any output from the api
    props?: { [key: string]: string };
};

export type Citation = {
    content: string;
    id: string;
    title: string | null;
    documentId: string;
    pageNumber: string;
    filepath: string | null;
    url: string | null;
    metadata: string | null;
    chunk_id: string | null;
    reindex_id: string | null;
}

export type ChatAppResponse = {
    message: ResponseMessage;
    delta: ResponseMessage;
    context: ResponseContext;
    session_state: any;
};

export type DataPointsContent = 
{
    fileName:string;
    content: string;
    documentId: string;
    pageNumber: string;
    datum:string;
    members:string;
    subject:string;
    onderwerp:string;
    summary:string;
    questionsAndAnswers:IQuestions_And_Answers[];
}

export type ResponseContext = {
    data_points: DataPointsContent[];
    followup_questions: string[] | null;
    thoughts: Thoughts[];
};

export type ResponseMessage = {
    content: string;
    role: string;
};
export type ChatAppResponseOrError = {
    message: ResponseMessage;
    delta: ResponseMessage;
    context: ResponseContext;
    session_state: any;
    error?: string;
};
