export interface IChatMessage {
    id?: string,
    role: string,
    content: string,
    timestamp?: string,
    followupquestions?: string[],
    citations?: Citation[],
    dataPointsContent?: DataPointsContent[],
    thougths?: Thoughts[]
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
    content: string;
    documentId: string;
    pageNumber: string;
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
