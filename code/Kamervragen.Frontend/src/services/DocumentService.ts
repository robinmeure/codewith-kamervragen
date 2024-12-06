import { IDocument } from "../models/Document";

export class DocumentService {

    private readonly baseUrl = process.env.VITE_BACKEND_URL;

    public getDocumentsAsync = async (chatId: string, token: string): Promise<IDocument[]> => {
        try {
            const response = await fetch(`${this.baseUrl}/threads/${chatId}/documents`, {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });
            if (!response.ok) {
                throw new Error(`Error fetching chat: ${response.statusText}`);
            }
           
            const documents: IDocument[] = await response.json();
            return documents;
        } catch (error) {
            console.error('Failed to fetch chats:', error);
            throw error;
        }
    };

    public addDocumentsAsync = async ({ chatId, documents, token }: { chatId: string, documents: File[], token: string }): Promise<boolean> => {

        if (!chatId || !Array.isArray(documents) || documents.length === 0) {
            console.log('No chat or documents to upload');
            return false;
        }
        const formData = new FormData();
        documents.forEach(file => {
            formData.append('documents', file);
        });

        try {
            const response = await fetch(`${this.baseUrl}/threads/${chatId}/documents`, {
                method: 'POST',
                body: formData,
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });
            if (!response.ok) {
                return false;
            }
            return true;
        } catch (e) {
            console.log(e);
            return false;
        }
    }

    public deleteDocumentAsync = async ({chatId, documentId, token} : {chatId: string, documentId: string, token: string}): Promise<boolean> => {
        
        try {
            const response = await fetch(`${this.baseUrl}/threads/${chatId}/documents/${documentId}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`,
                }
            });
            if (!response.ok) {
                throw new Error(`Error deleting document: ${response.statusText}`);
            }
            return true;
        } catch (error) {
            console.error('Failed to create chat:', error);
            return false;
        }
    }
}