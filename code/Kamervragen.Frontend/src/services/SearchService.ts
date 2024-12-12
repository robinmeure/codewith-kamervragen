import { IDocumentResult } from "../models/DocumentResult";
import { ISearchDocument } from "../models/SearchDocument";

export class SearchService {

    private readonly baseUrl = process.env.VITE_BACKEND_URL;
   
    public getSearchResultsAsync = async (chatId: string, query:string, token: string): Promise<ISearchDocument[]> => {
        try {
            const response = await fetch(`${this.baseUrl}/threads/${chatId}/search?query=${query}`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });
            if (!response.ok) {
                throw new Error(`Error fetching chats: ${response.statusText}`);
            }
            const chats: ISearchDocument[] = await response.json();
            return chats;
        } catch (error) {
            console.error('Failed to fetch chats:', error);
        }
        return [];
    };

    public getAnswersAsync = async (chatId: string, documentId:string, token: string): Promise<IDocumentResult> => {
        try {
            const response = await fetch(`${this.baseUrl}/threads/${chatId}/search/${documentId}`, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });
            if (response.status == 404)
            {
                return null;
            }
            if (!response.ok) {
                throw new Error(`Error fetching answers: ${response.statusText}`);
            }
            const answers: IDocumentResult = await response.json();
            return answers;
        } catch (error) {
            console.error('Failed to fetch answers:', error);
        }
        return {} as IDocumentResult;
    };
}