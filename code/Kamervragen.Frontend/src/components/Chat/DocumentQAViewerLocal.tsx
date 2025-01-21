import React, { useState, useEffect, useCallback } from 'react';
import { IDocumentResult } from '../../models/DocumentResult';
import QuestionAnswerList, { SelectedQAPair } from '../Search/QuestionAnswerList';
import { useSearch } from '../../hooks/useSearch';
import { useAuth } from '../../hooks/useAuth';
import { Stack, Text } from '@fluentui/react';
import { TagGroup, Tag, makeStyles, tokens, Card, CardHeader, CardPreview } from '@fluentui/react-components';
import { DataPointsContent } from '../../models/ChatMessage';
import QuestionAnswerList2 from '../Search/QuestionAnswerList2';


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
  document: DataPointsContent;
  chatId: string | undefined;
  onQAPairsSelected: (selectedPairs: SelectedQAPair[]) => void;
}

const DocumentQAViewerLocal: React.FC<DocumentQAViewerProps> = ({ document, chatId, onQAPairsSelected }) => {
  const classes = useStyles();

  // Memoize the handler to prevent unnecessary re-renders
  const handleSelectionChange = useCallback(
    (selectedPairs: SelectedQAPair[]) => {
      onQAPairsSelected(selectedPairs);
    },
    [onQAPairsSelected]
  );

  if (!document) {
    return <p>No data available.</p>;
  }

  return (
    <div key={document.documentId}>
      <Card className={classes.supportingContentContainer}>
        <Text variant="large">{document.fileName} - {document.onderwerp}</Text>
        <div className={classes.highlights}>
            <Text>{document.summary}</Text>
        </div>
        <div className={classes.tags}>
        <TagGroup role="list">
            <Tag role="listitem" size="extra-small" className="mb-2 mr-2">
            {document.members}
            </Tag>
            <Tag role="listitem" size="extra-small" className="mb-2 mr-2">
            {document.datum}
            </Tag>
                                     
        </TagGroup>
        </div>
       
        <QuestionAnswerList2
            document={document}
            onSelectionChange={handleSelectionChange}
          />
      </Card>
      
      
      </div>
     
  );
};

export default DocumentQAViewerLocal;