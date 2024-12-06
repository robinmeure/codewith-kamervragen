import { useState, useCallback } from 'react';
import { useAuth } from './useAuth';
import { SearchService } from '../services/SearchService';
import { ISearchDocument } from '../models/SearchDocument';
import { IDocumentResult } from '../models/DocumentResult';

export const useSearch = (chatId: string | undefined) => {
  const { accessToken } = useAuth();
  const searchService = useCallback(() => new SearchService(), []);
  const [documents, setDocuments] = useState<ISearchDocument[]>([]);
  const [isLoading, setIsLoading] = useState(false);

  const searchDocuments = useCallback(async (query: string) => {
    if (!accessToken || !chatId) {
      console.warn('Access token or Chat ID is not available');
      return;
    }
    setIsLoading(true);
    try {
      const response = await searchService().getSearchResultsAsync(chatId, query, accessToken);
      setDocuments(response);
    } catch (error) {
      console.error('Failed to fetch search results:', error);
    } finally {
      setIsLoading(false);
    }
  }, [accessToken, chatId, searchService]);

  const getAnswers = useCallback(async (documentId: string): Promise<IDocumentResult | null> => {
    if (!accessToken || !chatId) {
      console.warn('Access token or Chat ID is not available');
      return null;
    }
    try {
      const response = await searchService().getAnswersAsync(chatId, documentId, accessToken);
      return response || null;
    } catch (error) {
      console.error('Failed to fetch answers:', error);
      return null;
    }
  }, [accessToken, chatId, searchService]);

  return {
    documents,
    isLoading,
    searchDocuments,
    getAnswers,
  };
};