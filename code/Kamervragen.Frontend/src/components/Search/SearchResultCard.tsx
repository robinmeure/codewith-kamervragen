import React, { useState } from 'react';
import { makeStyles, tokens, Text, Button, TagGroup, Tag } from '@fluentui/react-components';
import { ISearchDocument } from '../../models/SearchDocument';
import { IDocumentResult } from '../../models/DocumentResult';
import QuestionAnswerList, { SelectedQAPair } from './QuestionAnswerList';
import Markdown from 'react-markdown';
import QuestionAnswerList2 from './QuestionAnswerList2';

const useClasses = makeStyles({
  container: {
    backgroundColor: tokens.colorNeutralBackground3,
    display: 'flex',
    flexDirection: 'column',
    padding: '10px',
    borderRadius: '4px',
    marginBottom: '10px',
  },
  content: {
    flexGrow: 1,
  },
  highlights: {
    flexGrow: 1,
  },
  fileName: {
    fontWeight: 'bold',
    fontSize: '16px',
  },
  button: {
    marginTop: '10px',
  },
  tags: {
    marginTop: '10px',
    display: 'flex',
    flexWrap: 'wrap',
    borderTop: '1px solid #ccc',
    paddingTop: '10px',
  },
  readMoreButton: {
    marginTop: tokens.spacingVerticalXS,
  },
});

interface SearchResultCardProps {
  document: ISearchDocument;
  onQAPairsSelected: (documentId: string, selectedPairs: SelectedQAPair[]) => void;
  getAnswers: (documentId: string) => Promise<IDocumentResult | null>;
}

const SearchResultCard: React.FC<SearchResultCardProps> = ({ document, onQAPairsSelected, getAnswers }) => {
  const classes = useClasses();
  const [documentResult, setDocumentResult] = useState<IDocumentResult | null>(null);
  const [showQAList, setShowQAList] = useState(false);
  const [isExpanded, setIsExpanded] = useState(false);

  const fetchDocumentResult = async () => {
    // if (!documentResult) {
    //   const result = await getAnswers(document.documentId);
    //   setDocumentResult(result);
    // }
    setShowQAList(!showQAList);
  };

  const handleReadMore = () => {
    setIsExpanded(!isExpanded);
  };

 const truncatedContent =
    document.summary.length > 500 ? document.summary.substring(0, 500) + '...' : document.summary;

  const handleSelectionChange = (selectedPairs: SelectedQAPair[]) => {
    onQAPairsSelected(document.documentId, selectedPairs);
  };

  return (
    <div className={classes.container}>
      <Text as="h1" className={classes.fileName} title={document.onderwerp}>
        {document.fileName}
      </Text>
      <div className={classes.highlights}>
        <Text>{document.onderwerp}</Text>
      </div>
      <div className={classes.content}>
        <Markdown>{isExpanded ? document.summary : truncatedContent}</Markdown>
        {document.summary.length > 500 && (
          <Button appearance="transparent" className={classes.readMoreButton} onClick={handleReadMore}>
            {isExpanded ? 'Read less' : 'Read more'}
          </Button>
        )}
      </div>
      <div className={classes.tags}>
        <TagGroup role="list">
          <Tag role="listitem" size="extra-small" className="mb-2 mr-2">
            {document.members}
          </Tag>
          <Tag role="listitem" size="extra-small" className="mb-2 mr-2">
            {document.soort}
          </Tag>  
          <Tag role="listitem" size="extra-small" className="mb-2 mr-2">
            {document.fileName}
          </Tag>                               
        </TagGroup>
      </div>
      {/* {document.questionandanswers && (
        // <QuestionAnswerList
        //   document={document}
        //   onSelectionChange={handleSelectionChange}
        // />
      )} */}
      </div>   
  );
};

export default SearchResultCard;