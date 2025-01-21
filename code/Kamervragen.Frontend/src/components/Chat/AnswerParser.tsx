import { IChatMessage, Citation } from "../../models/ChatMessage";

interface MarkdownParsedAnswer {
  markdownText: string;
  citations: Citation[];
}

export function parseAnswerToMarkdown(answer: IChatMessage): MarkdownParsedAnswer {
    const citations: Citation[] = [];
    let markdownText = answer.content.trim();
  
    const citationRegex = /\[([0-9a-fA-F-]+)#(\d+)\]/g;
  
    let citationIndex = 0;
    markdownText = markdownText.replace(citationRegex, (match, documentId, pageNumber) => {
    const citation = answer.context?.citations?.find((c) => {
        console.log('Comparing citation:', {
            cDocumentId: c.documentId,
            cPageNumber: c.pageNumber,
            documentId,
            pageNumber,
        });
        return c.documentId === documentId && c.pageNumber === pageNumber;
        });
        
    if (!citation) {
    console.warn('Citation not found for:', { documentId, pageNumber });
    return match;
    }
  
    if (!citation) {
    return match;
    }

    let index = citations.findIndex(
    (c) => c.documentId === documentId && c.pageNumber === pageNumber
    ) + 1;
    
    if (index === 0) {
    citations.push(citation);
    citationIndex += 1;
    index = citationIndex;
    }

    // Use footnote reference
    return `[^${index}]`;
});
  
    const footnotes = citations
      .map((citation, idx) => `[^${idx + 1}]: ${citation.documentId || 'Citation'}`)
      .join('\n');
  
    return {
      markdownText: markdownText,
      citations,
    };
  }