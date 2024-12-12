import React, { useState, useEffect, useCallback } from 'react';
import { IDocumentResult } from '../../models/DocumentResult';
import QuestionAnswerList, { SelectedQAPair } from '../Search/QuestionAnswerList';
import { useSearch } from '../../hooks/useSearch';
import { useAuth } from '../../hooks/useAuth';
import { Stack, Text } from '@fluentui/react';
import { TagGroup, Tag, makeStyles, tokens, Card, CardHeader, CardPreview } from '@fluentui/react-components';


const useStyles = makeStyles({
  container: {
    padding: tokens.spacingVerticalM,
    width: '100%',
  },
  qaPair: {
    display: 'flex',
    alignItems: 'center',
    marginBottom: tokens.spacingVerticalS,
  },
  checkbox: {
    verticalAlign: 'top',
    marginRight: tokens.spacingHorizontalS,
  },
  accordionHeader: {
    backgroundColor: tokens.colorNeutralBackground3Hover,
},
accordionPanel: {
  paddingLeft: '1rem',
},
accordionItem: {
  margin: '1rem',
},
tags: {
  marginTop: '10px',
  display: 'flex',
  flexWrap: 'wrap',
  borderTop: '1px solid #ccc',
  paddingTop: '10px',
},
highlights: {
  flexGrow: 1,
},
supportingContentContainer: {
  backgroundColor: tokens.colorNeutralBackground3,
  borderRadius: tokens.borderRadiusXLarge,
  maxWidth: '80%',
  padding: tokens.spacingHorizontalM,
  marginTop: "20",
  marginBottom: "20px",
  boxShadow: tokens.shadow2
},
subheader: {
  marginTop: tokens.spacingVerticalS,
  paddingBottom: tokens.spacingVerticalXS,
  fontWeight: tokens.fontWeightRegular,
  fontSize: tokens.fontSizeBase200,
  color: tokens.colorNeutralForeground3
},
});

interface DocumentQAViewerProps {
  documentId: string;
  chatId: string | undefined;
  onQAPairsSelected: (selectedPairs: SelectedQAPair[]) => void;
}

const DocumentQAViewer: React.FC<DocumentQAViewerProps> = ({ documentId, chatId, onQAPairsSelected }) => {
  const { getAnswers, isLoading } = useSearch(chatId);
  const { accessToken } = useAuth();
  const [documentResult, setDocumentResult] = useState<IDocumentResult | null>(null);
  const classes = useStyles();


  // Memoize the fetch function to maintain stable reference
  const fetchDocumentResult = useCallback(async () => {
    if (!accessToken || !chatId) {
      console.warn('Access token or Chat ID is not available');
      return;
    }
    try {
      const result = await getAnswers(documentId);
      if (result === null || result === undefined) {
        console.warn('No answers found for document:', documentId);
        return;
      }
      setDocumentResult(result);
    } catch (error) {
      console.error('Error fetching document answers:', error);
    }
  }, [accessToken, chatId, documentId, getAnswers]);

  useEffect(() => {
    fetchDocumentResult();
  }, [fetchDocumentResult]);

  // Memoize the handler to prevent unnecessary re-renders
  const handleSelectionChange = useCallback(
    (selectedPairs: SelectedQAPair[]) => {
      onQAPairsSelected(selectedPairs);
    },
    [onQAPairsSelected]
  );

  if (isLoading) {
    return <p>Loading...</p>;
  }

  if (!documentResult) {
    return <p>No data available.</p>;
  }

  return (
    <div key={documentResult.id}>
      <Card className={classes.supportingContentContainer}>
          <Text variant="large" weight="bold">{documentResult.title}</Text>
          <div className={classes.highlights}>
            <Text>{documentResult.subject}</Text>
          </div>
          <div className={classes.tags}>
            <TagGroup role="list">
              <Tag role="listitem" size="extra-small" className="mb-2 mr-2">
                Tweede Kamer
              </Tag>
              <Tag role="listitem" size="extra-small" className="mb-2 mr-2">
              {documentResult.date}
              </Tag>                               
            </TagGroup>
          </div>
       
        <QuestionAnswerList
            document={documentResult}
            onSelectionChange={handleSelectionChange}
          />
      </Card>
      
      
      </div>
     
  );
};

export default DocumentQAViewer;