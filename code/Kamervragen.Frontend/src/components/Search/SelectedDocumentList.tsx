import React from 'react';
import { makeStyles, tokens, Text, Card, CardHeader } from '@fluentui/react-components';
import { SelectedQAPair } from '../Search/QuestionAnswerList';

const useClasses = makeStyles({
  container: {
    display: 'flex',
    flexDirection: 'column',
    width: '70%',
    borderRadius: tokens.borderRadiusMedium,
    padding: tokens.spacingVerticalM,
    marginTop: tokens.spacingVerticalL
  },
  scrollContainer: {
    flex: 1,
    heigth: '100%',
    display: 'flex',
    overflow: 'scroll',
    overflowX: 'hidden',
    flexDirection: 'column',
    '&::-webkit-scrollbar': {
        display: 'none'
    },
  },
  qaPair: {
    marginBottom: tokens.spacingVerticalS,
  },
  question: {
    fontWeight: tokens.fontWeightSemibold,
  },
  answer: {
    marginLeft: tokens.spacingHorizontalL,
  },
  card: {
    width: '100%',
    maxWidth: '100%',
    marginBottom: tokens.spacingVerticalS
  },
  qaContent: {
    padding: tokens.spacingVerticalS,
  },
});

interface SelectedDocumentsListProps {
  selectedQAPairs: SelectedQAPair[];
}

const SelectedDocumentsList: React.FC<SelectedDocumentsListProps> = ({ selectedQAPairs }) => {
  const classes = useClasses();

  return (
    <div className={classes.scrollContainer}>
        <div className={classes.container}>
        {selectedQAPairs.map((qa, index) => (
        <Card key={index} className={classes.card}>
        <CardHeader
            header={<Text weight="semibold">Document ID: {qa.documentId}</Text>}
            description="Selected Q&A"
        />
        <div className={classes.qaContent}>
            <Text className={classes.question}>Q: {qa.question}</Text>
            {qa.answer && <Text className={classes.answer}>A: {qa.answer}</Text>}
        </div>
        </Card>
        ))}
        </div>
    </div>
    
  );
};

export default SelectedDocumentsList;